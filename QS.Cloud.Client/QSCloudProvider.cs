using QS.Cloud.Core;
using QS.DbManagement;
using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Data.SqlTypes;
using MySqlConnector;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public class QSCloudProvider : IDbProvider {
		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool IsConnected => throw new NotImplementedException();

		public ConnectionInfo ConnectionInfo { get; }

		public bool IsAdmin { get; protected set; }

		public string UserName { get; private set; }

		private CloudFeaturesClient featuresClient;
		private LoginManagementCloudClient loginClient;
		private SessionManagementCloudClient sessionClient;
		private UserManagementCloudClient userClient;


		public QSCloudProvider(QSCloudConnectionInfo connection) {
			ConnectionInfo = connection;
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

		public List<DbInfo> GetUserDatabases() {
			return loginClient.GetBasesForUser().Select(bi => new DbInfo
			{
				Title = bi.BaseTitle,
				BaseId = bi.BaseId
			}).ToList();
		}

		public LoginToDatabaseResponce LoginToDatabase(DbInfo dbInfo) {
			LoginToDatabaseResponce resp;

			try {
				var cloudResponce = loginClient.StartSession(dbInfo.BaseId);
				var builder = new MySqlConnectionStringBuilder {
					Server = cloudResponce.Db.Server,
					UserID = cloudResponce.Db.Login,
					Password = cloudResponce.Db.Password,
					Database = cloudResponce.Db.BaseName
				};
				if(!string.IsNullOrEmpty(cloudResponce.Db.Port))
					builder.Port = uint.Parse(cloudResponce.Db.Port);
				resp = new LoginToDatabaseResponce {
					Success = cloudResponce.Success,
					ConnectionString = builder.ConnectionString,
					Parameters = new List<ConnectionParameter>() { new ConnectionParameter("SessionId", cloudResponce.SessionId) }
				};
			}
			catch(Exception ex) {
				resp = new LoginToDatabaseResponce {
					Success = false,
					ErrorMessage = ex.Message
				};
			}

			return resp;
		}

		public LoginToServerResponce LoginToServer(LoginToServerData loginToServerData) {

			UserName = loginToServerData.UserName;

			string organisation = ConnectionInfo.Parameters.First(p => p.Title == "Логин").Value.ToString();
			BasicAuthInfoProvider authInfo = new BasicAuthInfoProvider($@"{organisation}\{loginToServerData.UserName}", loginToServerData.Password);

			loginClient = new LoginManagementCloudClient(authInfo);

			LoginToServerResponce resp;

			StartResponse cloudResponce;
			try {
				cloudResponce = loginClient.Start("0.1.0.0");

				resp = new LoginToServerResponce {
					Success = true,
					IsAdmin = cloudResponce.YouAccountAdmin,
					NeedToUpdateLauncher = cloudResponce.YouAccountAdmin
				};
			}
			catch(RpcException ex) {

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


