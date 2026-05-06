using Grpc.Core;
using MySqlConnector;
using QS.Cloud.Core;
using QS.DbManagement.Responces;
using QS.DbManagement;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Threading;
using QS.Cloud.Client.Clients;
using QS.DBScripts.Controllers;
using System.Threading.Tasks;

namespace QS.Cloud.Client.DataBase
{
	public class QSCloudProvider : IDbProvider {

		/// <summary>
		/// Публичный - в типе родключения нужен доступ, реализацию он знает и так
		/// </summary>
		public int BaseId { get; private set; }
		public BasicAuthInfoProvider AuthInfo { get; private set; }

		public bool IsConnected => throw new NotImplementedException();

		public bool IsAdmin { get; protected set; }

		#region Параметры подключени
		public string Account { get; private set; }

		#endregion
		public string UserName { get; private set; }

		public bool CanCreateDatabase => throw new NotImplementedException();

		private LoginManagementCloudClient loginClient;
		private DataBaseManagementCloudClient dbClient;


		public QSCloudProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			Account = parameters.First(p => p.Name == "Account").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			AuthInfo = new BasicAuthInfoProvider($@"{Account}\{UserName}", password);
			
			loginClient = new LoginManagementCloudClient(AuthInfo);
			dbClient = new DataBaseManagementCloudClient(AuthInfo);
		}

		public bool AddUser(string username, string password)
		{
			throw new NotImplementedException();
		}
	
		public bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}
	
		public bool CreateDatabase(string databaseName, string title)
		{
			CreateDataBaseResponse response = dbClient.CreateDataBase(databaseName, title);
			BaseId = response.BaseId;
			return true;
		}
	
		public void Dispose()
		{
			loginClient.Dispose();
		}
	
		public bool DropDatabase(string databaseName)
		{
			throw new NotImplementedException();
		}

		public List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo) {
			return loginClient.GetBasesForUser(applicationInfo.ProductCode).Select(bi => new DbInfo
			{
				Title = bi.BaseTitle,
				BaseId = bi.BaseId,
				Version = bi.BaseVersion
			}).ToList();
		}

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			LoginToDatabaseResponse resp;

			try {
				var cloudResponse = loginClient.StartSession(dbInfo.BaseId);
				var builder = new MySqlConnectionStringBuilder {
					Server = cloudResponse.Db.Server,
					Port = cloudResponse.Db.Port,
					UserID = cloudResponse.Db.Login,
					Password = cloudResponse.Db.Password,
					Database = cloudResponse.Db.BaseName
				};
				resp = new LoginToDatabaseResponse {
					Success = cloudResponse.Success,
					ConnectionString = builder.ConnectionString,
					Login = UserName,
					Parameters = new Dictionary<string, string>() { 
						{"SessionId", cloudResponse.SessionId},
						{"BaseTitle", dbInfo.Title}
					}
				};
			}
			catch(Exception ex) {
				resp = new LoginToDatabaseResponse {
					Success = false,
					ErrorMessage = ex.Message
				};
			}

			return resp;
		}

		public LoginToServerResponse LoginToServer() {
			LoginToServerResponse resp;

			StartResponse cloudResponce;
			try {
				cloudResponce = loginClient.Start(Assembly.GetExecutingAssembly().GetName().Version.ToString());

				resp = new LoginToServerResponse {
					Success = true,
					IsAdmin = cloudResponce.YouAccountAdmin,
					NeedToUpdateLauncher = cloudResponce.NeedUpdateLauncher
				};
			}
			catch(RpcException ex) when(ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated || ex.StatusCode == Grpc.Core.StatusCode.PermissionDenied) {
				resp = new LoginToServerResponse {
					Success = false,
					ErrorMessage = "Неверные данные для входа: " + ex.Message
				};
			}

			return resp;
		}
	}
}


