using System;

namespace QS.DbManagement
{
	public interface IDbProvider : IDisposable
	{
		string ConnectionString { get; set; }
	
		bool ChangePassword(string username, string oldPassword, string newPassword);
		
		bool CreateDatabase(string databaseName);
		
		bool DropDatabase(string databaseName);
		
		bool AddUser(string username, string password);
	
		bool IsConnected { get; }
	}
}
