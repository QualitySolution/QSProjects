using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace QSSaaS
{
	[ServiceContract]
	public interface ISaaSService
	{
		[OperationContract]
		/// <summary>
		/// Авторизация пользователя с выдачей идентификатора сессии и созданием соответствующей записи в журнале.
		/// </summary>
		/// <returns>Идентификатор сессии или сообщение об ошибке.</returns>
		/// <param name="login">Логин пользователя.</param>
		/// <param name="pass">Пароль пользователя.</param>
		/// <param name="account">Учетная запись.</param>
		/// <param name="bd">Название базы данных.</param>
		UserAuthorizeResult authorizeUser (string args);

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
		/// Регистрация пользователя с указанием его отображаемого имени.
		/// </summary>
		/// <returns>True в случае успеха или false и описание ошибки в случае неудачи.</returns>
		/// <param name="userLogin">Логин.</param>
		/// <param name="userPass">Пароль.</param>
		/// <param name="session">ID текущей сессии.</param>
		Result registerUserV2 (string userLogin, string userPass, string session, string userName);

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
		[WebGetAttribute (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Изменяет уровень доступа пользователя к базе данных.
		/// </summary>
		/// <returns><c>true</c>, при успешном выполнении, <c>false</c> в противном случае.</returns>
		/// <param name="sessionId">Идентификатор сессии.</param>
		/// <param name="user">Имя пользователя.</param>
		/// <param name="db">Название базы данных.</param>
		/// <param name="grant">Если <c>true</c> - устанавливает или обновляет права доступа. В противном случае - отбирает.</param>
		/// <param name="isAdmin">Дает административные права, если <c>true</c>.</param>
		bool changeBaseAccessFromProgram (string sessionId, string user, string db, bool grant, bool isAdmin = false);

		[OperationContract]
		[WebGet (ResponseFormat = WebMessageFormat.Json)]
		/// <summary>
		/// Создает пользователя в конечной базе данных.
		/// </summary>
		/// <returns><c>true</c>, если пользователь был создан, <c>false</c> в противном случае.</returns>
		/// <param name="sessionId">Идентификатор сессии.</param>
		bool createUserInBase (string sessionId);

		#region Obsolete

		[Obsolete ("Данный метод устарел и может быть удален в ближайшем будущем. " +
		"Он не позволяет получить реальное название базы и работает только в случае их совпадения." +
		"Рекомендуется использовать authorizeUser(string args) в котором данная проблема исправлена.")]
		[OperationContract]
		/// <summary>
		/// Авторизация пользователя с выдачей идентификатора сессии и созданием соответствующей записи в журнале.
		/// </summary>
		/// <returns>Идентификатор сессии или сообщение об ошибке.</returns>
		/// <param name="login">Логин пользователя.</param>
		/// <param name="pass">Пароль пользователя.</param>
		/// <param name="account">Учетная запись.</param>
		/// <param name="bd">Название базы данных.</param>
		UserAuthResult authUser (string args);

		[Obsolete ("Данный метод устарел и может быть удален в ближайшем будущем. " +
		"Вместо него следует использовать changeBaseAccessFromProgram с аргументом grant=true.")]
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

		[Obsolete ("Данный метод устарел и может быть удален в ближайшем будущем. " +
		"Вместо него следует использовать changeBaseAccessFromProgram с аргументом grant=false.")]
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

		#endregion
	}
}

