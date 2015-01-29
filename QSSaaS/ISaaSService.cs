using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSSaaS
{
	[ServiceContract]
	public interface ISaaSService
	{
		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Авторизация аккаунта с выдачей идентификатора сессии и созданием соответствующей записи в журнале.
		/// </summary>
		/// <returns>Идентификатор сессии или сообщение об ошибке.</returns>
		/// <param name="login">Логин аккаунта.</param>
		/// <param name="pass">Пароль аккаунта.</param>
		AccountAuthResult authAccount (string login, string pass);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Авторизация пользователя с выдачей идентификатора сессии и созданием соответствующей записи в журнале.
		/// </summary>
		/// <returns>Идентификатор сессии или сообщение об ошибке.</returns>
		/// <param name="login">Логин пользователя.</param>
		/// <param name="pass">Пароль пользователя.</param>
		/// <param name="account">Учетная запись.</param>
		/// <param name="bd">Название базы данных.</param>
		UserAuthResult authUser (string login, string pass, string account, string db);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Регистрация учетной записи.
		/// </summary>
		/// <returns>True в случае успеха или false и описание ошибки в случае неудачи.</returns>
		/// <param name="login">Логин.</param>
		/// <param name="pass">Пароль.</param>
		/// <param name="customer">Наименование клиента.</param>
		/// <param name="email">Email клиента.</param>
		Result registerAccount (string login, string pass, string customer, string email);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Регистрация пользователя
		/// </summary>
		/// <returns>True в случае успеха или false и описание ошибки в случае неудачи.</returns>
		/// <param name="userLogin">Логин.</param>
		/// <param name="userPass">Пароль.</param>
		/// <param name="accountLogin">Логин аккаунта, к которому будет осуществляться привязка..</param>
		/// <param name="accountPass">Пароль аккаунта, к которому будет осуществляться привязка.</param>
		Result registerUser (string userLogin, string userPass, string accountLogin, string accountPass);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Продлевает сессию еще на 20 минут.
		/// </summary>
		/// <returns><c>true</c>, если сессия была обновлена <c>false</c> если нет.</returns>
		/// <param name="session">ID сессии.</param>
		bool refreshSession (string session);
	}
}

