using QS.DbManagement;

using System;

namespace QS.Cloud.Client
{
	public class QSCloudConnection : IDbProvider
	{
		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	
		public bool IsConnected => throw new NotImplementedException();
	
		public bool AddUser(string username, string password)
		{
			throw new NotImplementedException();
		}
	
		public bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}
	
		public bool CreateDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}
	
		public void Dispose()
		{
			throw new NotImplementedException();
		}
	
		public bool DropDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}
	}

}


