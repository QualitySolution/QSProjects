using System;
using System.Linq;
using MySqlConnector;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	
	/// <summary>
	/// Класс предназначен для отображения понятных сообщений пользователю при проблемах связи.
	/// В отличии от класса ConnectionIsLost, предназначен для обработки сообщений из библиотеки MySqlConnector
	/// </summary>
	public class MySqlConnectorConnectionIsLost : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IErrorReporter errorReporter;
		private readonly IErrorReportingSettings settings;

		public MySqlConnectorConnectionIsLost(IInteractiveMessage interactiveMessage, IErrorReporter errorReporter = null, IErrorReportingSettings settings = null) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.errorReporter = errorReporter;
			this.settings = settings;
		}

		public bool Take(Exception exception) {
			var mysqlExceptions = exception.FindAllExceptionTypeInInner<MySqlException>().ToArray();
			if(mysqlExceptions.Any(e => e.Number == 1042)) {
				var message = String.Join("\n", mysqlExceptions.Select(e => e.Message));
				interactiveMessage.ShowMessage(ImportanceLevel.Error, 
					message + "\n\nУбедитесь что сеть или интернет работают и повторите попытку. Если соединение восстановить " +
					"не удасться, обратитесь к вашему системному администратору.",
					"Невозможно подключиться к БД");
				if(settings?.SendAutomatically ?? false)
					errorReporter?.SendReport(exception, ErrorType.Known);
				return true;
			}

			return false;
		}
	}
}
