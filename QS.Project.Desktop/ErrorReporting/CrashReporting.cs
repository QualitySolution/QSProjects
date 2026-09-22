using System;
using System.Threading;

namespace QS.ErrorReporting {
	/// <summary>
	/// последний перехват ошибок, срабатывает на исключении, которое никто не обработал и из-за которого процесс закрывается.
	/// спасти процесс здесь уже нельзя, поэтому ошибка пишется в лог и отчёт отправляется автоматически, без вопроса пользователю.
	/// ошибки, после которых приложение продолжает работать, ловят обработчики потока GUI.
	/// сюда доходит только то, что прошло мимо них.
	/// </summary>
	/// <remarks>
	/// создать и вызвать <see cref="Subscribe"/> в самом начале Main, чтобы поймать и ошибки запуска.
	/// пока контейнер не собран, <see cref="Reporter"/> не задан и падение остаётся только в логе.
	/// после сборки контейнера задать <see cref="Reporter"/> и <see cref="Settings"/>.
	/// отправка идёт в отдельном потоке, процесс ждёт её не дольше <see cref="SendTimeout"/>.
	/// </remarks>
	public class CrashReporting {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string SendThreadName = "Отправка отчёта об ошибке";

		private static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(3);

		private bool subscribed;

		/// <summary>
		/// Куда отправлять
		/// </summary>
		public IErrorReporter Reporter { get; set; }

		/// <summary>Не задано - отправляем</summary>
		public IErrorReportingSettings Settings { get; set; }

		/// <summary>Сколько падающий процесс ждёт отправку, прежде чем закрыться</summary>
		public TimeSpan SendTimeout { get; set; } = TimeSpan.FromSeconds(15);

		public void Subscribe() {
			if(subscribed)
				return;

			AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandled;
			subscribed = true;
		}

		public void Unsubscribe() {
			if(!subscribed)
				return;

			AppDomain.CurrentDomain.UnhandledException -= OnDomainUnhandled;
			subscribed = false;
		}

		private void OnDomainUnhandled(object sender, UnhandledExceptionEventArgs e) {
			try {
				var exception = ToException(e.ExceptionObject);
				logger.Fatal(exception, "Падение процесса, завершается: {0}", e.IsTerminating);

				// в отчёт входит хвост журнала - сбрасываем буферы, иначе строка выше в него не попадёт
				NLog.LogManager.Flush(FlushTimeout);

				if(!ShouldSend())
					return;

				// отправка в своём потоке - только так падающий поток может её не дождаться
				if(!StartSend(exception).Join(SendTimeout))
					logger.Warn("Отчёт не успели отправить за {0} с, отпускаем процесс", SendTimeout.TotalSeconds);
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось разобрать падение процесса");
			}
		}

		private bool ShouldSend() {
			if(Reporter == null) {
				logger.Debug("Отправитель отчётов не задан - падение осталось только в журнале");
				return false;
			}

			if(Settings == null || Settings.SendAutomatically)
				return true;

			logger.Debug("Автоотправка выключена, отчёт о падении не отправляем");
			return false;
		}

		private Thread StartSend(Exception exception) {
			var thread = new Thread(() => TrySend(exception)) {
				IsBackground = true,
				Name = SendThreadName
			};
			thread.Start();
			return thread;
		}

		private void TrySend(Exception exception) {
			try {
				if(!Reporter.SendReport(exception, ErrorType.Automatic))
					logger.Warn("Отчёт о падении отправить не удалось");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при отправке отчёта о падении");
			}
		}

		private static Exception ToException(object exceptionObject) {
			var exception = exceptionObject as Exception;
			if(exception != null)
				return exception;

			return new InvalidOperationException($"Необработанный объект: {exceptionObject}");
		}
	}
}
