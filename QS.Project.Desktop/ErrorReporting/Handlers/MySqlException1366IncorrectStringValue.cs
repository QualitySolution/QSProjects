using System;
using MySqlConnector;
using QS.Dialog;
using QS.Utilities;

namespace QS.ErrorReporting.Handlers {
	public class MySqlException1366IncorrectStringValue : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public MySqlException1366IncorrectStringValue(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var mysqlEx = exception.FindExceptionTypeInInner<MySqlException>();
			if(mysqlEx != null && mysqlEx.Number == 1366) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "При сохранении в базу данных произошла ошибка «Incorrect string value», " +
				                                                      "обычно это означает что вы вставили в поле диалога какие-то символы не поддерживаемые текущей кодировкой поля таблицы. " +
				                                                      "Например вы вставили один из символов Эмодзи, при этом кодировка полей в таблицах у вас трех-байтовая utf8, " +
				                                                      "для того чтобы сохранять такие символы в MariaDB\\MySQL базу данных, вам необходимо преобразовать все таблицы в " +
				                                                      "четырех-байтовую кодировку utf8mb4. Кодировка utf8mb4 используется по умолчанию в новых версиях MariaDB.");
				return true;
			}
			return false;
		}
	}
}
