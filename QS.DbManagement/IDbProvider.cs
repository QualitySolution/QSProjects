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

		bool ChangePassword(string username, string oldPassword, string newPassword);

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

		bool AddUser(string username, string password);

		LoginToServerResponse LoginToServer();

		List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo);

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
		bool CanCreateDatabase { get; }
		bool CanDropDatabase { get; }
	}
}
