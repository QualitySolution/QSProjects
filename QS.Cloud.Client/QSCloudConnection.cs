using QS.DbManagement;

using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Cloud.Client
{
	public class QSCloudConnection : IDbProvider
	{
		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	
		public bool IsConnected => throw new NotImplementedException();

		public ConnectionInfo Connection { get; }

		public bool IsAdmin { get; protected set; }

		private CloudFeaturesClient featuresClient;
		private LoginManagementCloudClient loginClient;
		private SessionManagementCloudClient sessionClient;
		private UserManagementCloudClient userClient;


		public QSCloudConnection(ConnectionInfo connection) {
			Connection = connection;

			string password = Connection.Parameters.First(p => p.Title == "password").Value as string;
		}

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

		public List<DbInfo> GetUserDatabases(string username) {
			throw new NotImplementedException();
		}

		public bool LoginToDatabase(string databaseName, string username, string password) {
			throw new NotImplementedException();
		}

		public bool LoginToServer(LoginToServerData loginToServerData) {

			BasicAuthInfoProvider authInfo = new BasicAuthInfoProvider(loginToServerData.UserName, loginToServerData.Password);

			loginClient = new LoginManagementCloudClient(authInfo);
			var resp = loginClient.Start("0.0.1.0");

			IsAdmin = resp.YouAccountAdmin;
			return resp.YouAccountAdmin;
		}
	}

}


