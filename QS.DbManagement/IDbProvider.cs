using QS.Dialog;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Threading;
using System;
using QS.DbManagement.Entities;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		string UserName { get; }

		#region Управление пользователями

		bool ChangeOwnPassword(string newPassword);

		bool CanManageUsers { get; }

		DbUserFields SupportedUserFields { get; }

		List<DbUserInfo> GetUsers();

		bool CreateUser(DbUserInfo user, string password);

		bool UpdateUser(DbUserInfo user, string newPassword = null);

		bool DeleteUser(string login);

		List<DbUserBaseAccess> GetUserBaseAccess(string login, IApplicationInfo applicationInfo);

		bool SetUserBaseAccess(string login, DbUserBaseAccess access);

		#endregion

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

		LoginToServerResponse LoginToServer();

		List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo);

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
		bool CanCreateDatabase { get; }
		bool CanDropDatabase { get; }
	}
}
