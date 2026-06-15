using Grpc.Core;
using MySqlConnector;
using QS.Cloud.Core;
using QS.DbManagement.Responces;
using QS.DbManagement;
using QS.Dialog;
using QS.Project.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using System.Threading;
using QS.Cloud.Client.Clients;
using QS.DBScripts.Controllers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace QS.Cloud.Client.DataBase
{
	public class QSCloudProvider : IDbProvider {

		public int BaseId { get; private set; }
		public BasicAuthInfoProvider AuthInfo { get; private set; }

		public bool IsConnected { get; private set; }

		public bool IsAdmin { get; protected set; }

		#region Параметры подключени
		public string Account { get; private set; }

		#endregion
		public string UserName { get; private set; }

		public bool CanCreateDatabase => dbClient.CanConnect;

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
	
		public bool CreateDatabase(string databaseName, string title, IServiceProvider services)
		{
			IApplicationInfo applicationInfo = services.GetService<IApplicationInfo>();
			CreateDataBaseResponse response = dbClient.CreateDataBase(databaseName, title, applicationInfo);
			BaseId = response.BaseId;
			return true;
		}
	
		public void Dispose()
		{
			loginClient.Dispose();
		}
	
		public bool DropDatabase(DbInfo database)
		{
			// Удаление - лёгкая операция с бухгалтерией реестра, делаем на сервере унарным gRPC.
			var response = dbClient.DropDataBase(database.BaseId);
			return response.Success;
		}

		public void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation)
		{
			// Бэкап тяжёлый - берём временное подключение к базе через сессию и гоним экспорт локально,
			// тем же сервисом, что и MariaDB (по аналогии с наполнением при создании).
			var session = loginClient.StartSession(database.BaseId);
			if(!session.Success)
				throw new InvalidOperationException("Не удалось открыть сессию к облачной базе: " + session.Description);

			var sessionLife = new AliveCloudClient(new SessionInfoProvider(session.SessionId));
			sessionLife.KeepAlive();
			try {
				var builder = new MySqlConnectionStringBuilder {
					Server = session.Db.Server,
					Port = session.Db.Port,
					UserID = session.Db.Login,
					Password = session.Db.Password,
					Database = session.Db.BaseName,
					AllowUserVariables = true
				};
				new MariaDbBackupService().Backup(builder, session.Db.BaseName, filePath, progress, cancellation);
			}
			finally {
				sessionLife.Dispose();
			}
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


