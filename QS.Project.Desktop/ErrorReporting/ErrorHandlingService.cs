using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.Dialog;

namespace QS.ErrorReporting {

	/// <summary>
	/// Порядок обработчиков важен: первый, кто узнал ошибку, забирает её себе,
	/// до предложения отправить отчёт она уже не доходит.
	///
	/// <see cref="IErrorReporter"/> и настройки необязательны: без них приложение
	/// просто показывает сообщение, как и раньше, - отсутствие отправки отчётов
	/// не повод ломать показ ошибки.
	/// </summary>
	public class ErrorHandlingService : IErrorHandlingService {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string DefaultTitle = "Непредвиденная ошибка";
		private const string SendButton = "Отправить отчёт";
		private const string CloseButton = "Закрыть";

		private readonly IEnumerable<IErrorHandler> handlers;
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IErrorReporter errorReporter;
		private readonly IErrorReportingSettings settings;

		public ErrorHandlingService(
			IEnumerable<IErrorHandler> handlers,
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			IErrorReporter errorReporter = null,
			IErrorReportingSettings settings = null)
		{
			this.handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.errorReporter = errorReporter;
			this.settings = settings;
		}

		public void Handle(Exception exception, string operationTitle = null) {
			if(exception == null)
				return;

			string title = string.IsNullOrEmpty(operationTitle) ? DefaultTitle : operationTitle;
			logger.Error(exception, "Ошибка при выполнении операции: {0}", title);

			if(TakenByHandler(exception))
				return;

			ShowUnexpected(exception, title);
		}

		/// <summary>Сбой самого обработчика не должен подменять исходную ошибку - идём к следующему</summary>
		private bool TakenByHandler(Exception exception) {
			foreach(var handler in handlers) {
				try {
					if(handler.Take(exception))
						return true;
				}
				catch(Exception ex) {
					logger.Error(ex, "Ошибка в обработчике {0}", handler.GetType().Name);
				}
			}
			return false;
		}

		private void ShowUnexpected(Exception exception, string title) {
			if(errorReporter == null) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, exception.Message, title);
				return;
			}

			if(settings != null && settings.SendAutomatically) {
				SendQuietly(exception, ErrorType.Automatic);
				interactiveMessage.ShowMessage(ImportanceLevel.Error,
					exception.Message + "\n\nОтчёт об ошибке отправлен разработчикам.", title);
				return;
			}

			// Вопрос блокирующий, и задавать его из GUI-потока нельзя, а Handle зовут
			// синхронно откуда угодно. Уводим в фон и не ждём: показать ошибку мы уже
			// обязались, а отправка отчёта вызывающего не касается
			_ = Task.Run(() => AskAndSend(exception, title));
		}

		private void AskAndSend(Exception exception, string title) {
			try {
				string answer = interactiveQuestion.Question(
					new[] { SendButton, CloseButton },
					exception.Message + "\n\nОтправить отчёт об ошибке разработчикам? " +
					"В отчёт попадут описание ошибки и последние строки журнала работы программы.",
					title);

				if(answer == SendButton)
					SendQuietly(exception, ErrorType.User);
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось показать сообщение о непредвиденной ошибке");
			}
		}

		/// <summary>Не отправился отчёт - это не повод показывать пользователю вторую ошибку поверх первой</summary>
		private void SendQuietly(Exception exception, ErrorType type) {
			try {
				if(!errorReporter.SendReport(exception, type))
					logger.Warn("Отчёт об ошибке отправить не удалось");
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при отправке отчёта об ошибке");
			}
		}
	}
}
