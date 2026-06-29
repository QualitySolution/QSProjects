using Grpc.Core;
using MySqlConnector;
using QS.Cloud.Core;
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
using QS.DbManagement.Entities;

namespace QS.Cloud.Client.DataBase
{
	public class QSCloudProvider : IDbProvider {

		public bool IsConnected { get; private set; }

		public bool IsAdmin { get; protected set; }

		#region Параметры подключени
		public string Account { get; private set; }

		#endregion
		public string UserName { get; private set; }

		public bool CanCreateDatabase => dbClient.CanConnect && IsAdmin;
		public bool CanDropDatabase => CanCreateDatabase;

		private LoginManagementCloudClient loginClient;
		private DataBaseManagementCloudClient dbClient;


		public QSCloudProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			Account = parameters.First(p => p.Name == "Account").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			var authInfo = new BasicAuthInfoProvider($@"{Account}\{UserName}", password);

			loginClient = new LoginManagementCloudClient(authInfo);
			dbClient = new DataBaseManagementCloudClient(authInfo);
		}

		public bool AddUser(string username, string password)
		{
			throw new NotImplementedException();
		}
	
		public bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}
	
		public bool CreateDatabase(DbCreationRequest request)
		{
			if(request == null)
				throw new ArgumentNullException(nameof(request));

			// 1. Создаём базу в облаке (gRPC). BaseId нужен только локально — открыть сессию.
			var response = dbClient.CreateDataBase(request.DbName, request.DbTitle, request.ApplicationInfo);

			// 2. Открываем временную сессию к созданной базе и наполняем её тем же механизмом, что и MariaDB.
			using(var session = CloudDbSession.Open(loginClient, response.BaseId)) {
				if(!session.Success) {
					request.Interaction.ReportError("Не удалось открыть сессию к созданной базе: " + session.Description, "Создание базы в облаке");
					return false;
				}
				if(!session.IsAdmin) {
					request.Interaction.ReportError("Вы не имеете прав администратора для наполнения базы.", "Создание базы в облаке");
					return false;
				}

				var filler = request.FillStrategy.CreateFiller(new DbFillResources {
					ConnectionString = session.ConnectionStringBuilder.ConnectionString,
					Progress = request.Progress,
					CancellationToken = request.CancellationToken,
				});
				return filler.RunCreation(session.Db.BaseName, request.DbTitle);
			}
		}
	
		public void Dispose()
		{
			loginClient.Dispose();
		}
	
		public bool DropDatabase(DbInfo database)
		{
			var response = dbClient.DropDataBase(database.BaseId);
			return response.Success;
		}

		public void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation)
		{
			using(var session = CloudDbSession.Open(loginClient, database.BaseId)) {
				if(!session.Success)
					throw new InvalidOperationException("Не удалось открыть сессию к облачной базе: " + session.Description);
				new MariaDbDumpService().Export(session.ConnectionStringBuilder, session.Db.BaseName, filePath, progress, cancellation);
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

				IsAdmin = cloudResponce.YouAccountAdmin;
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


