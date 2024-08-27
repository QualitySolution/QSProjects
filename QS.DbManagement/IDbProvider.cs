using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		ConnectionInfo Connection { get; }
	
		bool ChangePassword(string username, string oldPassword, string newPassword);
		
		bool CreateDatabase(string databaseName);
		
		bool DropDatabase(string databaseName);
		
		bool AddUser(string username, string password);

		LoginToServerResponce LoginToServer(LoginToServerData loginToServerData);

		List<DbInfo> GetUserDatabases();

		bool LoginToDatabase(string databaseName, string username, string password);
	
		bool IsConnected { get; }

		bool IsAdmin { get; }
	}
}
