using Grpc.Core;
using MySqlConnector;
using QS.Cloud.Client.Clients;
using QS.Cloud.Core;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
		private UserManagementCloudClient userClient;

		public QSCloudProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			Account = parameters.First(p => p.Name == "Account").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			var authInfo = new BasicAuthInfoProvider($@"{Account}\{UserName}", password);

			loginClient = new LoginManagementCloudClient(authInfo);
			dbClient = new DataBaseManagementCloudClient(authInfo);
			userClient = new UserManagementCloudClient(authInfo);
		}

		#region Управление пользователями

		public bool CanManageUsers => IsAdmin;

		public DbUserFields SupportedUserFields =>
			DbUserFields.Name | DbUserFields.Email | DbUserFields.Phone | DbUserFields.Post
			| DbUserFields.Comment | DbUserFields.AdminFlag | DbUserFields.Disabling | DbUserFields.BaseReadOnly;

		public bool ChangeOwnPassword(string newPassword) {
			try {
				return loginClient.ChangePassword(newPassword).Success;
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public List<DbUserInfo> GetUsers() {
			try {
				return userClient.GetUsers().Select(u => new DbUserInfo {
					Login = u.Login,
					Name = u.Name,
					Email = u.Email,
					Phone = u.Phone,
					Post = u.Post,
					Comment = u.Comment,
					Disabled = u.Disabled,
					IsAdmin = u.IsAccountAdmin,
					IsCurrentUser = string.Equals(u.Login, UserName, StringComparison.OrdinalIgnoreCase)
				}).ToList();
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public bool CreateUser(DbUserInfo user, string password) {
			try {
				var response = userClient.CreateUser(ToCloudUser(user), password);
				if(!response.Success)
					throw new InvalidOperationException(response.Message);
				return true;
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public bool UpdateUser(DbUserInfo user, string newPassword = null) {
			try {
				var response = userClient.UpdateUser(ToCloudUser(user), newPassword);
				if(!response.Success)
					throw new InvalidOperationException(response.Message);
				return true;
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public bool DeleteUser(string login) {
			try {
				var response = userClient.DeleteUser(login);
				if(!response.Success)
					throw new InvalidOperationException(response.Message);
				return true;
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public List<DbUserBaseAccess> GetUserBaseAccess(string login, IApplicationInfo applicationInfo) {
			try {
				return userClient.GetUserBaseAccess(login, applicationInfo.ProductCode).Select(b => new DbUserBaseAccess {
					BaseId = b.BaseId,
					Title = b.BaseTitle,
					HasAccess = b.HasAccess,
					IsAdmin = b.Admin,
					ReadOnly = b.ReadOnly
				}).ToList();
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		public bool SetUserBaseAccess(string login, DbUserBaseAccess access, IApplicationInfo applicationInfo) {
			try {
				bool ok = userClient.ChangeBaseAccess(login, access.BaseId, access.HasAccess, access.IsAdmin, access.ReadOnly, applicationInfo.ProductCode);
				if(!ok)
					throw new InvalidOperationException("Не удалось изменить доступ к базе");
				return true;
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		private static QS.Cloud.Core.UserInfo ToCloudUser(DbUserInfo user) => new QS.Cloud.Core.UserInfo {
			Login = user.Login ?? "",
			Name = user.Name ?? "",
			Email = user.Email ?? "",
			Phone = user.Phone ?? "",
			Post = user.Post ?? "",
			Comment = user.Comment ?? "",
			Disabled = user.Disabled,
			IsAccountAdmin = user.IsAdmin
		};

		private static Exception CloudError(RpcException ex) =>
			new InvalidOperationException(string.IsNullOrEmpty(ex.Status.Detail) ? ex.Message : ex.Status.Detail);

		#endregion
	
		public bool CreateDatabase(DbCreationRequest request) {
			if(request == null)
				throw new ArgumentNullException(nameof(request));

			var response = dbClient.CreateDataBase(request.DbName, request.DbTitle, request.ApplicationInfo);

			using(var session = CloudDbSession.Open(loginClient, response.BaseId)) {
				if(!session.Success) {
					request.Interaction.ReportError("Не удалось открыть сессию к созданной базе: " + session.Description, "Создание базы в облаке");
					return false;
				}
				if(!session.IsAdmin) {
					request.Interaction.ReportError("Вы не имеете прав администратора для наполнения базы", "Создание базы в облаке");
					return false;
				}

				return request.CreationModel.RunCreation(
					session.ConnectionStringBuilder.ConnectionString,
					session.Db.BaseName, request.DbTitle,
					request.Progress, request.CancellationToken);
			}
		}

		public bool ImportDatabase(DbImportRequest request) {
			if(request == null)
				throw new ArgumentNullException(nameof(request));

			var response = dbClient.CreateDataBase(request.DbName, request.DbTitle, request.ApplicationInfo);

			using(var session = CloudDbSession.Open(loginClient, response.BaseId)) {
				if(!session.Success) {
					request.Interaction.ReportError("Не удалось открыть сессию к созданной базе: " + session.Description, "Импорт базы в облако");
					return false;
				}
				if(!session.IsAdmin) {
					request.Interaction.ReportError("Вы не имеете прав администратора для наполнения базы", "Импорт базы в облако");
					return false;
				}

				request.DumpService.Import(
					session.ConnectionStringBuilder.ConnectionString, session.Db.BaseName, request.DumpFilePath,
					request.Progress, request.CancellationToken, request.DbTitle);
				request.CancellationToken.ThrowIfCancellationRequested();
				return true;
			}
		}
	
		public void Dispose()
		{
			loginClient.Dispose();
			dbClient.Dispose();
			userClient.Dispose();
		}
	
		public bool DropDatabase(DbInfo database, IApplicationInfo applicationInfo)
		{
			var response = dbClient.DropDataBase(database.BaseId, applicationInfo);
			return response.Success;
		}

		public void BackupDatabase(DbInfo database, string filePath, IDbDumpService dumpService, IProgressBarDisplayable progress, CancellationToken cancellation)
		{
			using(var session = CloudDbSession.Open(loginClient, database.BaseId)) {
				if(!session.Success)
					throw new InvalidOperationException("Не удалось открыть сессию к облачной базе: " + session.Description);
				dumpService.Export(session.ConnectionStringBuilder.ConnectionString, session.Db.BaseName, filePath, progress, cancellation);
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


