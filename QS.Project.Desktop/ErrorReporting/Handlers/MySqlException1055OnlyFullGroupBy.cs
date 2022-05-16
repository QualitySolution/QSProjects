using System;
using MySql.Data.MySqlClient;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	public class MySqlException1055OnlyFullGroupBy : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public MySqlException1055OnlyFullGroupBy(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var mysqlEx = exception.FindExceptionTypeInInner<MySqlException>();
			if(mysqlEx != null && mysqlEx.Number == 1055) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "На сервере MariaDB\\MySQL включен режим 'only_full_group_by', " +
				                                                      "для нормальной работы программы нужно удалить это значение из опции sql_mode. Обычно по умолчанию этот режим " +
				                                                      "отключен, проверьте свой конфигурационный файл my.cnf");
				return true;
			}
			return false;
		}
	}
}
