using System;
using System.Linq;
using MySqlConnector;
using QS.Dialog;
using QS.Utilities.Debug;

namespace QS.ErrorReporting.Handlers {

	/// <summary>
	/// Сервер отказал в доступе, неверный логин или пароль, нет прав на базу
	/// </summary>
	public class MySqlExceptionLoginFailed : IErrorHandler
	{
		private static readonly int[] AccessDeniedNumbers = {
			1044 //нет доступа к базе
			, 1045 //неверный логин или пароль
			, 1698 //учётка есть, но входить ей этим способом нельзя
		};

		private readonly IInteractiveMessage interactiveMessage;

		public MySqlExceptionLoginFailed(IInteractiveMessage interactiveMessage) {
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
		}

		public bool Take(Exception exception) {
			var mysqlEx = exception.FindAllExceptionTypeInInner<MySqlException>()
				.FirstOrDefault(e => AccessDeniedNumbers.Contains(e.Number));
			if(mysqlEx == null)
				return false;

			interactiveMessage.ShowMessage(ImportanceLevel.Warning,
				mysqlEx.Message + "\n\nПроверьте имя пользователя и пароль. " +
				"Если данные верны, обратитесь к администратору сервера баз данных.",
				"Сервер отказал в доступе");
			return true;
		}
	}
}
