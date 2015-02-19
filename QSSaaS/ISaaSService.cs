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
		AccountInfo getAccountInfo (string session_id);

		[OperationContract]
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
		/// <param name="session">ID текущей сессии.</param>
		Result registerUser (string userLogin, string userPass, string session);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Продлевает сессию еще на 20 минут.
		/// </summary>
		/// <returns><c>true</c>, если сессия была обновлена <c>false</c> если нет.</returns>
		/// <param name="session">ID сессии.</param>
		bool refreshSession (string session);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Изменяет пароль пользователя в системе SaaS.
		/// </summary>
		/// <returns><c>true</c>, если пароль был успешно изменен, в противном случае <c>false</c>.</returns>
		/// <param name="session">ID сессии.</param>
		/// <param name="newPassword">Новый пароль пользователя.</param>
		bool changeUserPasswordBySessionId (string session, string newPassword);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Изменяет пароль пользователя в системе SaaS.
		/// </summary>
		/// <returns><c>true</c>, если пароль был успешно изменен, в противном случае <c>false</c>.</returns>
		/// <param name="login">Логин пользователя.</param>
		/// <param name="account">Логин учетной записи.</param>
		/// <param name="newPassword">Новый пароль пользователя.</param>
		bool changeUserPasswordByLogin (string login, string account, string newPassword);


		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Предоставляет пользователю доступ к базе.
		/// </summary>
		/// <returns><c>true</c>, если предоставление доступа прошло успешно, иначе <c>false</c>.</returns>
		/// <param name="user">Логин пользователя.</param>
		/// <param name="account">Логин аккаунта.</param>
		/// <param name="db">Название базы данных.</param>
		/// <param name="admin">Имеет ли пользователь административный доступ.</param>
		bool grantBaseAccess (string user, string account, string db, bool admin);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Снимает доступ к базе для пользователя.
		/// </summary>
		/// <returns><c>true</c>, если операция прошла успешно, иначе <c>false</c>.</returns>
		/// <param name="user">Логин пользователя.</param>
		/// <param name="account">Логин аккаунта.</param>
		/// <param name="db">Название базы данных.</param>
		bool revokeBaseAccess (string user, string account, string db);
	}
}

