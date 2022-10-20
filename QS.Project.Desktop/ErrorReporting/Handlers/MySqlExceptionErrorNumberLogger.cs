using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	/// <summary>
	/// Класс не является полноценным обработчиком событий, завершающих обработку.
	/// Он скорее является Middleware записывающий в лог номер ошибки в MySqlException.
	/// Поэтому очень желательно этот обработчик ставить одним из первых.
	/// </summary>
	public class MySqlExceptionErrorNumberLogger : IErrorHandler {
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public MySqlExceptionErrorNumberLogger() {
		}

		public bool Take(Exception exception) {
			var list = new List<string>();
			var mysqlEx = exception.FindExceptionTypeInInner<MySqlException>();
			while(mysqlEx != null) {
				list.Add($"{mysqlEx.Number}: {mysqlEx.Message}");
				if(mysqlEx.InnerException == null)
					break;
				mysqlEx = mysqlEx.InnerException.FindExceptionTypeInInner<MySqlException>();
			}
			if(list.Any())
				logger.Debug("MySqlException error numbers:\n" + String.Join("\n", list));
			return false;
		}
	}
}
