using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;

namespace QS.ErrorReporting.Handlers {
	/// <summary>
	/// Дополнительный класс для работы с MySqlConnector.MySqlException
	/// Пока расположен в QS.HistoryLog потому что только эта библиотека использует MySqlConnector
	/// Возможно в будущем надо будет переместить в другое место.
	/// 
	/// Класс не является полноценным обработчиком событий, завершающих обработку.
	/// Он скорее является Middleware записывающий в лог номер ошибки в MySqlException.
	/// Поэтому очень желательно этот обработчик ставить одним из первых.
	/// </summary>
	public class MySqlConnectorExceptionErrorNumberLogger : IErrorHandler {
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public MySqlConnectorExceptionErrorNumberLogger() {
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
				logger.Debug("MySqlConnector.MySqlException error numbers:\n" + String.Join("\n", list));
			return false;
		}
	}
}
