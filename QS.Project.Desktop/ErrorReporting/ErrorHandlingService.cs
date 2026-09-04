using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QS.Dialog;

namespace QS.ErrorReporting {
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

			// Вопрос блокирующий, и задавать его из UI-потока нельзя, а Handle зовут синхронно откуда угодно
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
