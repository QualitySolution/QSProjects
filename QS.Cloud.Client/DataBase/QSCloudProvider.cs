using Grpc.Core;
using MySqlConnector;
using QS.Cloud.Client.Clients;
using QS.Cloud.Core;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace QS.Cloud.Client.DataBase {
	public class QSCloudProvider : IDbProvider {

		public bool IsAdmin { get; protected set; }
		public string Account { get; private set; }
		public string UserName { get; private set; }
		public uint ProductCode { get; private set; }

		public bool CanCreateDatabase => dbClient.CanConnect && IsAdmin;
		public bool CanDropDatabase => CanCreateDatabase;
		private const string MessageTitle = "Создание базы в облаке";

		private readonly LoginManagementCloudClient loginClient;
		private readonly DataBaseManagementCloudClient dbClient;
		private readonly UserManagementCloudClient userClient;

		public QSCloudProvider(IList<ConnectionParameterValue> parameters, byte productCode, string password = null) {
			Account = parameters.First(p => p.Name == "Account").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			ProductCode = productCode;
			var authInfo = new BasicAuthInfoProvider($@"{Account}\{UserName}", password);

			loginClient = new LoginManagementCloudClient(authInfo);
			dbClient = new DataBaseManagementCloudClient(authInfo, ProductCode);
			userClient = new UserManagementCloudClient(authInfo);
		}

		#region Управление пользователями

		public bool CanManageUsers => IsAdmin;
		public bool CanManageBaseAccess => IsAdmin;

		public DbUserFields SupportedUserFields =>
			DbUserFields.Name | DbUserFields.Email | DbUserFields.Phone | DbUserFields.Post
			| DbUserFields.Comment | DbUserFields.AdminFlag | DbUserFields.Disabling | DbUserFields.BaseReadOnly;

		public bool ChangeOwnPassword(string newPassword) => Call(() =>
			loginClient.ChangePassword(newPassword).Success);

		public List<DbUserInfo> GetUsers() => Call(() =>
			userClient.GetUsers().Select(ToDbUserInfo).ToList());

		public bool CreateUser(DbUserInfo user, string password) => Call(() => {
				var response = userClient.CreateUser(ToCloudUser(user), password);
				return EnsureSuccess(response.Success, response.Message);
			});

		public bool UpdateUser(DbUserInfo user, string newPassword = null) => Call(() => {
				var response = userClient.UpdateUser(ToCloudUser(user), newPassword);
				return EnsureSuccess(response.Success, response.Message);
			});

		public bool DeleteUser(string login) =>
			Call(() => {
				var response = userClient.DeleteUser(login);
				return EnsureSuccess(response.Success, response.Message);
			});

		public List<DbUserBaseAccess> GetUserBaseAccess(string login) => Call(() =>
			userClient.GetUserBaseAccess(login, ProductCode)
				.Select(b => new DbUserBaseAccess {
					BaseId = b.BaseId,
					Title = b.BaseTitle,
					HasAccess = b.HasAccess,
					IsAdmin = b.Admin,
					ReadOnly = b.ReadOnly
				}).ToList());

		public bool SetUserBaseAccess(string login, DbUserBaseAccess access) =>
			Call(() => {
				var response = userClient.ChangeBaseAccess(
					login, access.BaseId, access.HasAccess, access.IsAdmin, access.ReadOnly, ProductCode);
				return EnsureSuccess(response.Success, string.IsNullOrEmpty(response.Message)
					? "Не удалось изменить доступ к базе"
					: response.Message);
			});

		private static T Call<T>(Func<T> operation) {
			try {
				return operation();
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		private static bool EnsureSuccess(bool success, string message) {
			if(!success)
				throw new InvalidOperationException(message);
			return true;
		}

		private DbUserInfo ToDbUserInfo(UserInfo user) => new DbUserInfo {
			Login = user.Login,
			Name = user.Name,
			Email = user.Email,
			Phone = user.Phone,
			Post = user.Post,
			Comment = user.Comment,
			Disabled = user.Disabled,
			IsAdmin = user.IsAccountAdmin,
			IsCurrentUser = string.Equals(user.Login, UserName, StringComparison.OrdinalIgnoreCase)
		};

		private static UserInfo ToCloudUser(DbUserInfo user) => new UserInfo {
			Login = user.Login ?? "",
			Name = user.Name ?? "",
			Email = user.Email ?? "",
			Phone = user.Phone ?? "",
			Post = user.Post ?? "",
			Comment = user.Comment ?? "",
			Disabled = user.Disabled,
			IsAccountAdmin = user.IsAdmin
		};

		private static Exception CloudError(RpcException ex) => new InvalidOperationException(Describe(ex));

		#endregion

		#region Управление базами

		public bool CanRefreshMetadata => false;

		public RefreshMetadataResponse RefreshMetadata() => throw new NotImplementedException();

		public bool CreateDatabase(DbCreationRequest request) {
			if(request == null)
				throw new ArgumentNullException(nameof(request));
			try {
				int? baseId = PrepareEmptyDatabase(request);
				if(baseId == null)
					return false;

				using(var session = CloudDbSession.Open(loginClient, baseId.Value)) {
					if(!session.Success) {
						request.Interaction.ReportError("Не удалось открыть сессию к созданной базе: " + session.Description, MessageTitle);
						return false;
					}
					if(!session.IsAdmin) {
						request.Interaction.ReportError("Вы не имеете прав администратора для наполнения базы", MessageTitle);
						return false;
					}

					request.CreationResources.ConnectionString = session.ConnectionStringBuilder.ConnectionString;
					request.CreationResources.JustCreated = true;
					var creationModel = request.CreationFactory.Create(request.CreationResources);
					return creationModel.RunCreation(session.Db.BaseName, request.DbTitle);
				}
			}
			catch(RpcException ex) {
				throw CloudError(ex);
			}
		}

		private int? PrepareEmptyDatabase(DbCreationRequest request) {
			var existing = dbClient.CheckDataBaseExists(request.DbName);
			if(!existing.Exists)
				return dbClient.CreateDataBase(request.DbName, request.DbTitle).BaseId;

			switch(request.Interaction.AskDropExistingDatabase(request.DbName)) {
				case ToDoWithExistingDatabase.Recreate:
					if(!dbClient.DropDataBase(existing.BaseId).Success) {
						request.Interaction.ReportError("Не удалось удалить существующую базу: " + existing.BaseId, MessageTitle);
						return null;
					}
					return dbClient.CreateDataBase(request.DbName, request.DbTitle).BaseId;

				case ToDoWithExistingDatabase.Rewrite:
					// облако пересоздаст пустую базу, сохранив записи реестра и права доступа
					if(!dbClient.ClearDataBase(existing.BaseId).Success) {
						request.Interaction.ReportError("Не удалось очистить существующую базу: " + existing.BaseId, MessageTitle);
						return null;
					}
					return existing.BaseId;

				default: // Nothing
					return null;
			}
		}

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if(disposed)
				return;

			if(disposing) {
				loginClient.Dispose();
				dbClient.Dispose();
				userClient.Dispose();
			}

			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool DropDatabase(DbInfo database) {
			var response = dbClient.DropDataBase(database.BaseId);
			return response.Success;
		}

		public void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation) {
			using(var session = CloudDbSession.Open(loginClient, database.BaseId)) {
				if(!session.Success)
					throw new InvalidOperationException("Не удалось открыть сессию к облачной базе: " + session.Description);
				new MariaDbExportService().Export(session.ConnectionStringBuilder, session.Db.BaseName, filePath, progress, cancellation);
			}
		}

		public List<DbInfo> GetUserDatabases() =>
			loginClient.GetBasesForUser(ProductCode).Select(bi => new DbInfo {
				Title = bi.BaseTitle,
				BaseId = bi.BaseId,
				BaseName = bi.BaseName,
				Version = bi.BaseVersion
			}).ToList();

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
					Parameters = new Dictionary<string, string>(StringComparer.Ordinal) {
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
			// вход - ожидаемая точка отказа, поэтому не исключение, а Response с текстом для пользователя
			try {
				var cloudResponce = loginClient.Start(Assembly.GetExecutingAssembly().GetName().Version.ToString());

				IsAdmin = cloudResponce.YouAccountAdmin;
				return new LoginToServerResponse {
					Success = true,
					IsAdmin = cloudResponce.YouAccountAdmin,
					NeedToUpdateLauncher = cloudResponce.NeedUpdateLauncher
				};
			}
			catch(RpcException ex) when(ex.StatusCode == StatusCode.Unauthenticated || ex.StatusCode == StatusCode.PermissionDenied) {
				return new LoginToServerResponse {
					Success = false,
					ErrorMessage = "Неверные данные для входа: " + Describe(ex)
				};
			}
			catch(RpcException ex) {
				return new LoginToServerResponse {
					Success = false,
					ErrorMessage = "Не удалось подключиться к облаку QS: " + Describe(ex)
				};
			}
		}

		/// <summary>Пояснение от сервера, если оно есть - оно понятнее сообщения самого gRPC.</summary>
		private static string Describe(RpcException ex) =>
			string.IsNullOrEmpty(ex.Status.Detail) ? ex.Message : ex.Status.Detail;
		#endregion
	}
}
