using QS.DbManagement.Responces;
using QS.Dialog;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Threading;
using System;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		string UserName { get; }

		bool ChangePassword(string username, string oldPassword, string newPassword);

		bool CreateDatabase(string databaseName, string title, IServiceProvider services = null);

		bool DropDatabase(DbInfo database);

		void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation);

		bool AddUser(string username, string password);

		LoginToServerResponse LoginToServer();

		List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo);

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
		bool CanCreateDatabase { get; }
	}
}
