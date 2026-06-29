using QS.Dialog;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Threading;
using System;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		string UserName { get; }

		bool ChangePassword(string username, string oldPassword, string newPassword);

		/// <summary>
		/// Создаёт базу и сразу наполняет её
		/// </summary>
		bool CreateDatabase<CreationArgs>(DbCreationRequest<CreationArgs> request) where CreationArgs : DbCreationResources;

		bool DropDatabase(DbInfo database);

		void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation);

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
