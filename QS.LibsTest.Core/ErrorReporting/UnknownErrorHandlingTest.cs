using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Launcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace QS.LibsTest.Core.ErrorReporting {
	/// <summary>
	/// Что происходит с ошибкой, которую не узнал ни один <see cref="IErrorHandler"/>.
	/// Цепочка собирается не руками, а вызовом AddLauncherErrorHandling: если набор обработчиков
	/// в регистрации изменится и незнакомая ошибка вдруг окажется «разобранной» - тест упадёт.
	/// </summary>
	[TestFixture(TestOf = typeof(ErrorHandlingService))]
	public class UnknownErrorHandlingTest {
		private static readonly TimeSpan Patience = TimeSpan.FromSeconds(5);

		private const string SendButton = "Отправить отчёт";
		private const string OperationTitle = "Смена пароля";

		private IInteractiveMessage message;
		private IInteractiveQuestion question;
		private IErrorReporter reporter;
		private ErrorReportingSettings settings;
		private IErrorHandlingService handling;
		private IEnumerable<IErrorHandler> handlers;

		[SetUp]
		public void SetUp() {
			message = Substitute.For<IInteractiveMessage>();
			question = Substitute.For<IInteractiveQuestion>();
			reporter = Substitute.For<IErrorReporter>();
			reporter.SendReport(Arg.Any<Exception>(), Arg.Any<ErrorType>()).Returns(true);
			settings = new ErrorReportingSettings(
				requestEmail: false, requestDescription: false, sendAutomatically: true, logRowCount: null);

			var container = new ServiceCollection()
				.AddSingleton(message)
				.AddSingleton(question)
				.AddSingleton(reporter)
				.AddSingleton<IErrorReportingSettings>(settings)
				.AddLauncherErrorHandling()
				.BuildServiceProvider();

			handling = container.GetRequiredService<IErrorHandlingService>();
			handlers = container.GetServices<IErrorHandler>();
		}

		[Test(Description = "Ошибку из настоящего метода не узнаёт ни один обработчик - она доходит до отправки отчёта")]
		public void Handle_ExceptionFromProviderMethod_NoHandlerTakesIt_ReportSent() {
			// без обработчиков проверять было бы нечего
			Assume.That(handlers, Is.Not.Empty);
			var exception = ExceptionFromChangeOwnPassword();

			handling.Handle(exception, OperationTitle);

			reporter.Received(1).SendReport(exception, ErrorType.Automatic);
			// у каждого обработчика свой заголовок окна: сообщение ровно одно и с заголовком операции
			Assert.That(message.ReceivedCalls().Count(), Is.EqualTo(1),
				"незнакомую ошибку не должен показывать ни один обработчик");
			message.Received(1).ShowMessage(ImportanceLevel.Error,
				Arg.Is<string>(text => text.StartsWith(exception.Message, StringComparison.Ordinal)), OperationTitle);
		}

		[Test(Description = "Без автоотправки отчёт уходит только после согласия пользователя")]
		public void Handle_AutoSendDisabled_AsksUserBeforeSending() {
			settings.SendAutomatically = false;
			question.Question(Arg.Any<string[]>(), Arg.Any<string>(), Arg.Any<string>()).Returns(SendButton);
			var exception = ExceptionFromChangeOwnPassword();

			handling.Handle(exception, OperationTitle);

			// вопрос блокирующий и задаётся из чужого потока - без ожидания тест увидел бы пустоту
			Assert.That(WaitUntil(() => reporter.ReceivedCalls().Any(), Patience), Is.True,
				"после согласия пользователя отчёт обязан уйти");
			reporter.Received(1).SendReport(exception, ErrorType.User);
		}

		private static Exception ExceptionFromChangeOwnPassword() => RealSource.FromChangeOwnPassword();

		/// <summary>
		/// Настоящее исключение из настоящего метода: пустой пароль ломает контракт
		/// <see cref="MariaDBProvider.ChangeOwnPassword"/> ещё до обращения к серверу
		/// </summary>
		private static class RealSource {
			public static Exception FromChangeOwnPassword() {
				var parameters = new List<ConnectionParameterValue> {
					new ConnectionParameterValue(new ConnectionParameter("Server", "Сервер"), "localhost"),
					new ConnectionParameterValue(new ConnectionParameter("Login", "Пользователь"), "someone")
				};

				using(var provider = new MariaDBProvider(parameters, productCode: 1, password: "pass")) {
					try {
						provider.ChangeOwnPassword(string.Empty);
					}
					catch(ArgumentException ex) {
						return ex;
					}
				}
				throw new InvalidOperationException(
					"ChangeOwnPassword перестал проверять пустой пароль - тест потерял источник ошибки");
			}
		}

		private static bool WaitUntil(Func<bool> condition, TimeSpan timeout) {
			var elapsed = Stopwatch.StartNew();
			while(elapsed.Elapsed < timeout) {
				if(condition())
					return true;
				Thread.Sleep(20);
			}
			return condition();
		}
	}
}
