﻿using System;
using MySql.Data.MySqlClient;
using QS.Dialog;

namespace QS.ErrorReporting.Handlers {
	public class MySqlExceptionAccessDenied : IErrorHandler {
		private readonly IInteractiveMessage interactiveMessage;

		public MySqlExceptionAccessDenied(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var mysqlEx = exception.FindExceptionTypeInInner<MySqlException>();
			if(mysqlEx != null && mysqlEx.Number == 1142) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, "У вас нет прав на выполнение этой операции на уровне базы данных MariaDB\\MySQL.");
				return true;
			}
			return false;
		}
	}
}