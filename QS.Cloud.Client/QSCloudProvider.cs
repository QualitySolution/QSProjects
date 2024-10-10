using QS.Cloud.Core;
using QS.DbManagement;
using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Grpc.Core;

namespace QS.Cloud.Client
{
	public class QSCloudProvider : IDbProvider {
		public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool IsConnected => throw new NotImplementedException();

		public bool IsAdmin { get; protected set; }

		#region Параметры подключени
		public string Account { get; private set; }
		

		#endregion
		public string UserName { get; private set; }

		private CloudFeaturesClient featuresClient;
		private LoginManagementCloudClient loginClient;
		private SessionManagementCloudClient sessionClient;
		private UserManagementCloudClient userClient;


		public QSCloudProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			Account = parameters.First(p => p.Name == "Account").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			BasicAuthInfoProvider authInfo = new BasicAuthInfoProvider($@"{Account}\{UserName}", password);
			
			loginClient = new LoginManagementCloudClient(authInfo);
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
				var cloudResponse = loginClient.StartSession(dbInfo.BaseId);
				var builder = new MySqlConnectionStringBuilder {
					Server = cloudResponse.Db.Server,
					Port = cloudResponse.Db.Port,
					UserID = cloudResponse.Db.Login,
					Password = cloudResponse.Db.Password,
					Database = cloudResponse.Db.BaseName
				};
				resp = new LoginToDatabaseResponce {
					Success = cloudResponse.Success,
					ConnectionString = builder.ConnectionString,
					Parameters = new List<(string Name, string Value)> { ("SessionId", cloudResponse.SessionId) }
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

		public LoginToServerResponce LoginToServer() {
			LoginToServerResponce resp;

			StartResponse cloudResponce;
			try {
				cloudResponce = loginClient.Start("0.1.0.0");

				resp = new LoginToServerResponce {
					Success = true,
					IsAdmin = cloudResponce.YouAccountAdmin,
					NeedToUpdateLauncher = cloudResponce.NeedUpdateLauncher
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


