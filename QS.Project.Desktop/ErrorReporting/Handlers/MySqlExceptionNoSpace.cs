using System;
using MySqlConnector;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {
	public class MySqlExceptionNoSpace : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public MySqlExceptionNoSpace(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var mysqlEx = exception.FindExceptionTypeInInner<MySqlException>();
			if(mysqlEx != null && mysqlEx.Message.Contains("28 \"No space left on device\"")) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "На диске, где расположена база данных MariaDB\\MySQL, закончилось свободное место. Обратитесь к администратору базы данных для освобождения места или увеличения дискового пространства.");
				return true;
			}
			return false;
		}
	}
}
