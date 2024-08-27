using QS.Cloud.Core;
using QS.DbManagement;
using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Cloud.Client
{
	public static class QSDbProviderFactoryExtensions
	{
		public static IDbProvider CreateQSCloudProvider(this DbProviderFactory factory, ConnectionInfo connectionInfo)
			=> new QSCloudConnection(connectionInfo);
	}

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

		public LoginToServerResponce LoginToServer(LoginToServerData loginToServerData) {

			string organisation = Connection.Parameters.First(p => p.Title == "Логин").Value.ToString();
			BasicAuthInfoProvider authInfo = new BasicAuthInfoProvider($@"{organisation}\{loginToServerData.UserName}", loginToServerData.Password);

			loginClient = new LoginManagementCloudClient(authInfo);

			LoginToServerResponce resp;

			StartResponse cloudResponce;
			try {
				cloudResponce = loginClient.Start("0.0.1.0");

				resp = new LoginToServerResponce {
					Success = true,
					IsAdmin = cloudResponce.YouAccountAdmin,
					NeedToUpdateLauncher = cloudResponce.YouAccountAdmin
				};
			}
			catch(Grpc.Core.RpcException ex) {

				resp = new LoginToServerResponce {
					Success = false,
					ErrorMessage = ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated ?
						"Неверные данные для входа" : ex.StatusCode.ToString()
				};
			}

			return resp;
		}
	}
}


