using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;

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

		List<DbInfo> GetUserDatabases();

		LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
	}
}
