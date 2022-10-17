using System;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using NHibernate.Exceptions;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	public class ConnectionIsLost : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;
		private readonly IErrorReporter errorReporter;
		private readonly IErrorReportingSettings settings;

		public ConnectionIsLost(IInteractiveMessage interactiveMessage, IErrorReporter errorReporter = null, IErrorReportingSettings settings = null) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.errorReporter = errorReporter;
			this.settings = settings;
		}

		public bool Take(Exception exception) {
			var sEx = exception.FindExceptionTypeInInner<SocketException>();
			if(sEx != null) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, 
					sEx.Message + "\n\nУбедитесь что сеть или интернет работают и повторите попытку. Если соединение восстановить " +
					"не удасться, обратитесь к вашему системному администратору.",
					"Ошибка сети");
				if(settings?.SendAutomatically ?? false)
					errorReporter?.SendReport(exception, ErrorType.Known);
				return true;
			}

			var timeoutException = exception.FindExceptionTypeInInner<TimeoutException>();
			if(timeoutException?.Source == "MySql.Data") {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, 
					timeoutException.Message + "\n\nУбедитесь что сеть или интернет работают и повторите попытку. Если соединение восстановить " +
					"не удасться, обратитесь к вашему системному администратору.",
					"Время ожидания истекло");
				if(settings?.SendAutomatically ?? false)
					errorReporter?.SendReport(exception, ErrorType.Known);
				return true;
			}
			
			var mysqlException = exception.FindExceptionTypeInInner<MySqlException>();
			if(mysqlException?.Number == 1042) {
				var message = mysqlException.Message;
				if(mysqlException.InnerException != null)
					message += "\n" + mysqlException.InnerException.Message;
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
