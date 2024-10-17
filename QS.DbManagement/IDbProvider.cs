using QS.DbManagement.Responces;
using QS.Project.Versioning;
using System.Collections.Generic;
using System;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		string UserName { get; }
	
		bool ChangePassword(string username, string oldPassword, string newPassword);
		
		bool CreateDatabase(string databaseName);
		
		bool DropDatabase(string databaseName);
		
		bool AddUser(string username, string password);

		LoginToServerResponse LoginToServer();

		List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo);

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
	}
}
