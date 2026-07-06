using QS.Dialog;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Threading;
using System;
using QS.DbManagement.Entities;

namespace QS.DbManagement
{
	public interface IDbManager
	{
		string UserName { get; }

		bool IsConnected { get; }

		bool IsAdmin { get; }
		bool CanCreateDatabase { get; }
		bool CanDropDatabase { get; }

		LoginToServerResponse LoginToServer();

		List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo);

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);

		/// <summary>
		/// Создаёт базу и наполняет её скриптом создания
		/// </summary>
		bool CreateDatabase(DbCreationRequest request);

		/// <summary>
		/// Создаёт базу и наполняет её пользовательским дампом
		/// </summary>
		bool ImportDatabase(DbImportRequest request);

		bool DropDatabase(DbInfo database);

		void BackupDatabase(DbInfo database, string filePath, IDbDumpService dumpService, IProgressBarDisplayable progress, CancellationToken cancellation);
	}

	/// <summary>
	/// Управление пользователями сервера и собственным паролем.
	/// </summary>
	public interface IDbUserManager
	{
		bool ChangeOwnPassword(string newPassword);

		bool CanManageUsers { get; }

		/// <summary>
		/// Какие поля пользователя и виды доступа поддерживает провайдер
		/// </summary>
		DbUserFields SupportedUserFields { get; }

		List<DbUserInfo> GetUsers();

		bool CreateUser(DbUserInfo user, string password);

		bool UpdateUser(DbUserInfo user, string newPassword = null);

		bool DeleteUser(string login);

		/// <summary>
		/// Список баз продукта с текущим доступом указанного пользователя
		/// </summary>
		List<DbUserBaseAccess> GetUserBaseAccess(string login, IApplicationInfo applicationInfo);

		/// <summary>
		/// меняет доступ пользователя к базе согласно флагам <paramref name="access"/>
		/// </summary>
		bool SetUserBaseAccess(string login, DbUserBaseAccess access);
	}

	public interface IDbProvider : IDbManager, IDbUserManager, IDisposable {}
}
