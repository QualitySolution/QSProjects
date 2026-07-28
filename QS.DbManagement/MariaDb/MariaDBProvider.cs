using Dapper;
using MySqlConnector;
using QS.BaseParameters;
using QS.DbManagement.Entities;
using QS.DbManagement.MariaDb;
using QS.DbManagement.MariaDb.QSLauncher;
using QS.DBScripts.Controllers;
using QS.Dialog;
using QS.Project.DB;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace QS.DbManagement {
	public class MariaDBProvider : IDbProvider {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };

		private const string MessageTitle = "Создание базы данных";
		private const string AllPrivileges = "ALL PRIVILEGES";
		private const string AnyHost = "%";

		private readonly MySqlConnection connection;

		/// <summary>
		/// Публичный - в типе подключения нужен доступ, реализацию он знает и так
		/// </summary>
		public MySqlConnectionStringBuilder ConnectionStringBuilder { get; }

		public bool IsAdmin { get; private set; }

		public bool CanCreateDatabase { get; private set; }
		public bool CanDropDatabase { get; private set; }
		public bool CanManageBaseAccess { get; private set; }

		#region Параметры подключения
		public string Server { get; }
		public string UserName { get; }
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			Server = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;

			var address = Server.Split(':');
			ConnectionStringBuilder = new MySqlConnectionStringBuilder {
				Server = address[0],
				UserID = UserName,
				Password = password,
				AllowUserVariables = true
			};
			if(address.Length > 1 && uint.TryParse(address[1], out var port))
				ConnectionStringBuilder.Port = port;

			connection = new MySqlConnection(ConnectionStringBuilder.ConnectionString);
		}

		#region Управление базами

		public LoginToServerResponse LoginToServer() {
			try {
				EnsureOpen();

				var grants = connection.Query<string>("SHOW GRANTS FOR CURRENT_USER").ToList();

				IsAdmin = MySqlGrants.HasGlobalAdmin(grants);
				CanManageBaseAccess = MySqlGrants.HasGlobalGrantOption(grants);

				var privileges = new HashSet<string>(grants
					.Where(g => MySqlGrants.Scope(g) != null)
					.SelectMany(MySqlGrants.Privileges), StringComparer.Ordinal);

				CanCreateDatabase = IsAdmin || HasPrivilege(privileges, "CREATE");
				CanDropDatabase = IsAdmin || HasPrivilege(privileges, "DROP");

				return new LoginToServerResponse {
					Success = true,
					IsAdmin = IsAdmin,
					NeedToUpdateLauncher = false
				};
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось подключиться к MariaDB {0} как {1}", Server, UserName);
				return new LoginToServerResponse {
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		private static bool HasPrivilege(ICollection<string> privileges, string privilege)
			=> privileges.Contains(AllPrivileges) || privileges.Contains(privilege);

		public List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo) {
			return FromMetadataOrDirect(applicationInfo.ProductCode,
				metadata => metadata.Bases.GetBases(UserName).ToList(),
				() => GetUserDatabasesDirect(applicationInfo));
		}

		private List<DbInfo> GetUserDatabasesDirect(IApplicationInfo applicationInfo) {
			EnsureOpen();

			return connection.Query<string>("SHOW DATABASES")
				.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase)
				.Select(dbName =>
					ReadDbInfo(dbName, applicationInfo.ProductCode))
				.Where(db => db != null)
				.ToList();
		}

		private DbInfo ReadDbInfo(string dbName, byte productCode) { //? поторение LauncherBasesManagement.ReadBaseParameters но надо подумать как ответственность не нарушать
			Dictionary<string, string> parameters;
			try {
				var toBase = new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString) { Database = dbName };
				parameters = new ParametersService(new MySqlConnectionFactory(toBase.ConnectionString).OpenConnection).All;
			}
			catch(MySqlException ex) {
				logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
				return null;
			}

			if(!parameters.TryGetValue("ProductCode", out var code) || !byte.TryParse(code, out var baseProduct)
				|| baseProduct != productCode)
				return null;

			return new DbInfo {
				BaseName = dbName,
				Title = parameters.TryGetValue("BaseTitle", out var title) ? title : dbName,
				Version = parameters.TryGetValue("version", out var version) ? version : null
			};
		}

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			try {
				ConnectionStringBuilder.Database = dbInfo.BaseName;

				return new LoginToDatabaseResponse {
					Success = true,
					ConnectionString = ConnectionStringBuilder.ConnectionString,
					Login = UserName,
					Parameters = new Dictionary<string, string>(StringComparer.Ordinal) {
						{ "BaseTitle", dbInfo.Title }
					}
				};
			}
			catch(Exception ex) {
				return new LoginToDatabaseResponse {
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		public bool CreateDatabase(DbCreationRequest request) {
			if(request == null)
				throw new ArgumentNullException(nameof(request));
			EnsureOpen();

			if(!PrepareEmptyDatabase(request))
				return false;

			var connectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString) {
				Database = request.DbName
			};
			request.CreationResources.ConnectionString = connectionStringBuilder.ConnectionString;
			request.CreationResources.JustCreated = true;
			var creationModel = request.CreationFactory.Create(request.CreationResources); //? users теперь не ответственность модели наполнения
			if(!creationModel.RunCreation(request.DbName, request.DbTitle))
				return false;

			RegisterInLauncherMetadata(request);
			return true;
		}

		/// <summary>
		/// Готовит пустую базу под наполнение
		/// false - пользователь отказался что-либо делать с существующей базой
		/// </summary>
		private bool PrepareEmptyDatabase(DbCreationRequest request) {
			if(!DoesDataBaseExist(request.DbName)) {
				CreateEmptyDatabase(request.DbName);
				return true;
			}

			switch(request.Interaction.AskDropExistingDatabase(request.DbName)) {
				case ToDoWithExistingDatabase.Recreate:
					if(!DropDatabase(new DbInfo { BaseName = request.DbName }, request.ApplicationInfo)) {
						request.Interaction.ReportError("Не удалось удалить существующую базу: " + request.DbName, MessageTitle);
						return false;
					}
					break;
				case ToDoWithExistingDatabase.Rewrite:
					connection.Execute($"DROP DATABASE IF EXISTS `{MySqlEscape.Identifier(request.DbName)}`");
					break;
				default: // Nothing
					return false;
			}

			CreateEmptyDatabase(request.DbName);
			return true;
		}

		private void CreateEmptyDatabase(string dbName) =>
			connection.Execute($"CREATE DATABASE `{MySqlEscape.Identifier(dbName)}`");

		#region QSLauncher

		private bool HasQSLauncherAccess = true;
		public bool CanRefreshMetadata => CanCreateDatabase;

		private LauncherMetadataManagement LMM;
		private LauncherMetadataManagement CreateLauncherMetadata(int productCode) {
			HasQSLauncherAccess = true;
			if(LMM == null) {
				var builder = new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString);
				LMM = new LauncherMetadataManagement(builder, CanCreateDatabase, UserName, productCode);
			}
			return LMM;
		}

		private bool TryCreateMetadata(int productCode, out LauncherMetadataManagement metadata) {
			metadata = null;
			if(!HasQSLauncherAccess)
				return false;
			try {
				metadata = CreateLauncherMetadata(productCode);
				return true;
			}
			catch(Exception ex) {
				HasQSLauncherAccess = false;
				logger.Debug(ex, "QSLauncher база недоступна, используем прямой доступ к серверу");
				return false;
			}
		}

		private T FromMetadataOrDirect<T>(int productCode, Func<LauncherMetadataManagement, T> fromMetadata, Func<T> direct) {
			if(TryCreateMetadata(productCode, out var metadata)) {
				try {
					return fromMetadata(metadata);
				}
				catch(Exception ex) {
					logger.Debug(ex, "Не удалось прочитать из метабазы, работаем напрямую с сервером.");
				}
			}
			return direct();
		}

		// Синхронизация таблицы users без привязки к базе
		private BaseUsersManagement baseUsers;
		private BaseUsersManagement BaseUsers =>
			baseUsers ?? (baseUsers = new BaseUsersManagement(new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString)));

		public void RefreshMetadata(IApplicationInfo applicationInfo) {
			var metadata = CreateLauncherMetadata(applicationInfo.ProductCode);
			metadata.Bases.SyncBases();
			metadata.Users.SyncUsers(GetUsersDirect().Select(u => u.Login));
		}

		private void RegisterInLauncherMetadata(DbCreationRequest request) {
			try {
				CreateLauncherMetadata(request.ApplicationInfo.ProductCode)
					.CreateBaseWithCreatorAccess(new DbInfo { Title = request.DbTitle, BaseName = request.DbName });
			}
			catch(Exception ex) {
				logger.Warn(ex, "Не удалось зарегистрировать базу {0} в метабазе QSLauncher.", request.DbName);
			}
		}

		#region Отображение сущностей метабазы в публичные

		private DbUserInfo ToDbUserInfo(Entities.LauncherUserInfo u) => new DbUserInfo {
			Login = u.Login,
			Name = u.Name,
			Email = u.Email,
			Phone = u.Phone,
			Post = u.Post,
			Comment = u.Comment,
			Disabled = u.Disabled,
			IsAdmin = u.IsAccountAdmin,
			IsCurrentUser = string.Equals(u.Login, UserName, StringComparison.OrdinalIgnoreCase)
		};

		private static Entities.LauncherUserInfo ToLauncherUser(DbUserInfo u) => new Entities.LauncherUserInfo {
			Login = u.Login,
			Name = u.Name,
			Email = u.Email,
			Phone = u.Phone,
			Post = u.Post,
			Comment = u.Comment,
			Disabled = u.Disabled,
			IsAccountAdmin = u.IsAdmin
		};

		private static DbUserBaseAccess ToDbUserBaseAccess(Entities.BaseAccessRow r) => new DbUserBaseAccess {
			BaseId = r.BaseId,
			BaseName = r.BaseName,
			Title = r.BaseTitle,
			HasAccess = r.HasAccess,
			IsAdmin = r.Admin,
			ReadOnly = r.ReadOnly
		};

		private void ReflectInMetadata(Action<LauncherMetadataManagement> action, string operation, string subject) {
			if(!TryCreateMetadata(0, out var metadata))
				return;
			try {
				action(metadata);
			}
			catch(Exception ex) {
				logger.Warn(ex, "Не удалось отразить {0} ({1}) в метабазе.", operation, subject);
			}
		}

		private void ReflectUserUpdateInMetadata(DbUserInfo user, string newPassword) {
			ReflectInMetadata(m => {
				var target = m.Users.GetUserByLogin(user.Login);
				if(target == null)
					return;
				var launcherUser = ToLauncherUser(user);
				launcherUser.Id = target.Id;
				m.Users.UpdateUser(launcherUser, newPassword);
			}, "обновление", user.Login);
		}

		#endregion

		private void TryRefreshMetadata(IApplicationInfo applicationInfo) {
			try {
				RefreshMetadata(applicationInfo);
			}
			catch(Exception ex) {
				logger.Warn(ex, "Не удалось синхронизировать метабазу QSLauncher.");
			}
		}

		#endregion

		private bool DoesDataBaseExist(string dbName) {
			int exists = connection.ExecuteScalar<int>(
				"SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name;",
				new { name = dbName });
			if(exists > 0)
				return true;
			return false;
		}

		public bool DropDatabase(DbInfo database, IApplicationInfo applicationInfo) {
			EnsureOpen();

			connection.Execute($"DROP DATABASE IF EXISTS `{MySqlEscape.Identifier(database.BaseName)}`");
			CleanDatabasePrivileges(database.BaseName);
			TryRefreshMetadata(applicationInfo);
			return true;
		}

		private void CleanDatabasePrivileges(string dbName) {
			// в mysql.db имя может храниться с экранированными шаблонными символами (foo\_bar)
			var names = new[] { dbName, dbName.Replace("_", "\\_").Replace("%", "\\%") }
				.Distinct(StringComparer.Ordinal).ToArray();
			try {
				connection.Execute(
					"DELETE FROM mysql.db WHERE Db IN @names;" +
					"DELETE FROM mysql.tables_priv WHERE Db IN @names;" +
					"DELETE FROM mysql.columns_priv WHERE Db IN @names;" +
					"DELETE FROM mysql.procs_priv WHERE Db IN @names;" +
					"FLUSH PRIVILEGES;",
					new { names });
			}
			catch(MySqlException ex) {
				// у текущего пользователя может не быть прав на mysql.* - база уже удалена, не валим операцию
				logger.Warn(ex, "Не удалось вычистить права удалённой базы {0}.", dbName);
			}
		}

		/// <summary>
		/// Резервное копирование базы в скрипт
		/// Метод блокирующий - вызывать из фонового потока
		/// </summary>
		public void BackupDatabase(DbInfo database, string filePath, IProgressBarDisplayable progress, CancellationToken cancellation) {
			new MariaDbExportService().Export(ConnectionStringBuilder, database.BaseName, filePath, progress, cancellation);
		}

		#endregion

		#region Управление пользователями

		public DbUserFields SupportedUserFields =>
			DbUserFields.BaseReadOnly
			| DbUserFields.Name | DbUserFields.Email
			| (CanManageUsers && SupportsAccountLock ? DbUserFields.Disabling : DbUserFields.None)
			| (SupportsAdminFlag ? DbUserFields.AdminFlag : DbUserFields.None);

		public bool CanManageUsers => IsAdmin;

		private bool SupportsAdminFlag => CanManageUsers && CanManageBaseAccess;

		private static readonly string[] SystemUsers = { "root", "mariadb.sys", "mysql", "PUBLIC" };

		private readonly Dictionary<string, List<string>> userHosts = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		private bool? supportsAccountLock;
		private bool SupportsAccountLock {
			get {
				if(supportsAccountLock == null) {
					EnsureOpen();
					supportsAccountLock = connection.ExecuteScalar<long>(
						"SELECT COUNT(*) FROM information_schema.COLUMNS " +
						"WHERE TABLE_SCHEMA = 'mysql' AND TABLE_NAME = 'user' AND COLUMN_NAME = 'account_locked'") > 0;
				}
				return supportsAccountLock.Value;
			}
		}

		public bool ChangeOwnPassword(string newPassword) {
			if(string.IsNullOrEmpty(newPassword))
				throw new ArgumentException("Пароль не может быть пустым", nameof(newPassword));
			EnsureOpen();

			connection.Execute($"ALTER USER CURRENT_USER() IDENTIFIED BY '{MySqlHelper.EscapeString(newPassword)}'");
			return true;
		}

		public List<DbUserInfo> GetUsers() {
			// пользователей показываем из метабазы
			// при её отсутствии - реальные учётки сервера
			return FromMetadataOrDirect(0,
				metadata => metadata.Users.GetUsers().Select(ToDbUserInfo).ToList(),
				GetUsersDirect);
		}

		private List<DbUserInfo> GetUsersDirect() {
			EnsureOpen();

			string lockedColumn = SupportsAccountLock ? "account_locked" : "NULL";

			var rows = connection.Query<MySqlUserRow>(
				$"SELECT User AS Login, Host, {lockedColumn} AS AccountLocked, " +
				"Super_priv AS SuperPriv, Create_user_priv AS CreateUserPriv " +
				"FROM mysql.user").ToList();

			// один логин заведён на сервере под несколькими хостами
			userHosts.Clear();
			var result = new List<DbUserInfo>();
			foreach(var accounts in rows.Where(IsRealUser).GroupBy(r => r.Login, StringComparer.Ordinal)) {
				userHosts[accounts.Key] = accounts.Select(r => string.IsNullOrEmpty(r.Host) ? AnyHost : r.Host).ToList();
				result.Add(new DbUserInfo {
					Login = accounts.Key,
					Disabled = accounts.All(r => IsYes(r.AccountLocked)),
					IsAdmin = accounts.All(r => IsYes(r.SuperPriv) || IsYes(r.CreateUserPriv)),
					IsCurrentUser = string.Equals(accounts.Key, UserName, StringComparison.OrdinalIgnoreCase)
				});
			}
			return result;
		}

		// служебные учётки самого сервера пользователю приложения не показываем
		private static bool IsRealUser(MySqlUserRow row) =>
			!string.IsNullOrEmpty(row.Login)
			&& !row.Login.StartsWith("mysql.", StringComparison.OrdinalIgnoreCase)
			&& !SystemUsers.Contains(row.Login, StringComparer.OrdinalIgnoreCase);

		private static bool IsYes(string flag) => string.Equals(flag, "Y", StringComparison.OrdinalIgnoreCase);

		public bool CreateUser(DbUserInfo user, string password) {
			ValidateLogin(user?.Login);
			if(string.IsNullOrEmpty(password))
				throw new ArgumentException("Пароль не может быть пустым", nameof(password));
			EnsureOpen();

			string lockOption = user.Disabled && SupportsAccountLock ? " ACCOUNT LOCK" : string.Empty;
			string userSql = AccountOf(user.Login, AnyHost);
			string userlocalSql = AccountOf(user.Login, "localhost"); //?
			var statements = new List<string> {
				$"CREATE USER {userSql} IDENTIFIED BY '{MySqlHelper.EscapeString(password)}'{lockOption}; " +
				$"CREATE USER {userlocalSql} IDENTIFIED BY '{MySqlHelper.EscapeString(password)}'{lockOption}; "
			};
			if(SupportsAdminFlag && user.IsAdmin) {
				statements.Add(GrantAdmin(userSql));
				statements.Add(GrantAdmin(userlocalSql));
			}

			connection.Execute(string.Join(";", statements));
			userHosts[user.Login] = new List<string> { AnyHost };

			ReflectInMetadata(m => m.Users.CreateUser(ToLauncherUser(user), password), "создание", user.Login);
			return true;
		}

		public bool UpdateUser(DbUserInfo user, string newPassword = null) {
			ValidateLogin(user?.Login);
			EnsureOpen();

			// одним батчем по всем хостам логина
			var statements = HostsOf(user.Login)
				.SelectMany(host => UserChangeStatements(user, host, newPassword))
				.ToList();

			// реальные учётные операции - только если есть что менять; профиль отражаем в метабазу всегда
			if(statements.Count > 0)
				connection.Execute(string.Join(";", statements));

			ReflectUserUpdateInMetadata(user, newPassword);
			return true;
		}

		/// <summary>Что нужно выполнить на сервере для одного аккаунта «логин@хост».</summary>
		private IEnumerable<string> UserChangeStatements(DbUserInfo user, string host, string newPassword) {
			string account = AccountOf(user.Login, host);

			string options = string.Join(" ", AlterUserOptions(user, newPassword));
			if(options.Length > 0)
				yield return $"ALTER USER {account} {options}";

			if(SupportsAdminFlag && user.DirtyFields.HasFlag(DbUserFields.AdminFlag))
				yield return user.IsAdmin ? GrantAdmin(account) : RevokeAdmin(account);
		}

		// через ALTER USER меняются только пароль и блокировка учётки
		private IEnumerable<string> AlterUserOptions(DbUserInfo user, string newPassword) {
			if(!string.IsNullOrEmpty(newPassword))
				yield return $"IDENTIFIED BY '{MySqlHelper.EscapeString(newPassword)}'";
			if(SupportsAccountLock && user.DirtyFields.HasFlag(DbUserFields.Disabling))
				yield return user.Disabled ? "ACCOUNT LOCK" : "ACCOUNT UNLOCK";
		}

		public bool DeleteUser(string login) {
			ValidateLogin(login);
			EnsureOpen();

			connection.Execute(string.Join(";", HostsOf(login)
				.Select(host => $"DROP USER IF EXISTS {AccountOf(login, host)}")));
			userHosts.Remove(login);

			ReflectInMetadata(m => {
				var target = m.Users.GetUserByLogin(login);
				if(target != null)
					m.Users.DeleteUser(target);
			}, "удаление", login);
			return true;
		}

		public List<DbUserBaseAccess> GetUserBaseAccess(string login, IApplicationInfo applicationInfo) {
			return FromMetadataOrDirect(applicationInfo.ProductCode,
				metadata => {
					var user = metadata.Users.GetUserByLogin(login)
						?? throw new InvalidOperationException($"Пользователь {login} не найден в метабазе.");
					var list = metadata.Users.GetUserBaseAccess(user).Select(ToDbUserBaseAccess).ToList();
					FillUsersProfiles(list, login);
					return list;
				},
				() => GetUserBaseAccessDirect(login, applicationInfo));
		}

		private List<DbUserBaseAccess> GetUserBaseAccessDirect(string login, IApplicationInfo applicationInfo) {
			EnsureOpen();

			var grants = ReadGrantsByHost(login).Values.SelectMany(g => g).ToList();
			bool globalAdmin = MySqlGrants.HasGlobalAdmin(grants);

			var result = GetUserDatabasesDirect(applicationInfo)
				.Select(db => globalAdmin ? FullAccessByGlobalGrant(db) : AccessFromGrants(db, grants))
				.ToList();

			FillUsersProfiles(result, login);
			return result;
		}

		private static DbUserBaseAccess FullAccessByGlobalGrant(DbInfo db) => new DbUserBaseAccess {
			BaseName = db.BaseName,
			Title = db.Title,
			HasAccess = true,
			IsAdmin = true,
			CanEdit = false
		};

		private static DbUserBaseAccess AccessFromGrants(DbInfo db, IEnumerable<string> grants) {
			var access = new DbUserBaseAccess { BaseName = db.BaseName, Title = db.Title };

			var privileges = grants
				.Where(g => CoversDatabase(g, db.BaseName))
				.SelectMany(MySqlGrants.Privileges)
				.Where(p => p != "USAGE")
				.ToList();
			if(privileges.Count == 0)
				return access;

			access.HasAccess = true;
			if(privileges.Contains(AllPrivileges))
				access.IsAdmin = true;
			else if(privileges.All(IsReadOnlyPrivilege))
				access.ReadOnly = true;
			return access;
		}

		private static bool CoversDatabase(string grant, string baseName) {
			string scope = MySqlGrants.Scope(grant);
			if(scope == null)
				return false;
			return scope == "*" || string.Equals(MySqlEscape.UnescapePattern(scope), baseName, StringComparison.OrdinalIgnoreCase);
		}

		private static bool GrantedOnDatabase(string grant, string baseName) {
			string scope = MySqlGrants.Scope(grant);
			return scope != null && scope != "*"
				&& string.Equals(MySqlEscape.UnescapePattern(scope), baseName, StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsReadOnlyPrivilege(string privilege) =>
			privilege == "SELECT" || privilege == "LOCK TABLES" || privilege == "SHOW VIEW";

		private void FillUsersProfiles(IEnumerable<DbUserBaseAccess> accesses, string login) {
			foreach(var access in accesses.Where(a => a.HasAccess && !string.IsNullOrEmpty(a.BaseName))) {
				var profile = BaseUsers.TryGetProfile(access.BaseName, login);
				if(profile != null) {
					access.Name = profile.Name;
					access.Email = profile.Email;
				}
			}
		}

		public bool SetUserBaseAccess(string login, DbUserBaseAccess access, IApplicationInfo applicationInfo) {
			ValidateLogin(login);
			if(string.IsNullOrWhiteSpace(access?.BaseName))
				throw new ArgumentException("Не указано имя базы", nameof(access));
			EnsureOpen();

			var grantsByHost = ReadGrantsByHost(login);
			if(grantsByHost.Count == 0)
				throw new InvalidOperationException($"Пользователь {login} не найден на сервере.");

			if(MySqlGrants.HasGlobalAdmin(grantsByHost.Values.SelectMany(g => g)))
				throw new InvalidOperationException(
					$"У пользователя {login} глобальные права на весь сервер");

			var statements = grantsByHost
				.SelectMany(hostGrants => AccessStatements(login, hostGrants.Key, hostGrants.Value, access))
				.ToList();
			if(statements.Count > 0)
				connection.Execute(string.Join(";", statements));

			ReflectAccessInBase(login, access);
			ReflectAccessInMetadata(login, access);
			return true;
		}

		private static IEnumerable<string> AccessStatements(string login, string host, IEnumerable<string> grants, DbUserBaseAccess access) {
			string account = AccountOf(login, host);

			foreach(var grant in grants.Where(g => GrantedOnDatabase(g, access.BaseName))) {
				string pattern = $"`{MySqlEscape.Identifier(MySqlGrants.Scope(grant))}`.*";
				if(MySqlGrants.IsMeaningful(grant))
					yield return $"REVOKE {AllPrivileges} ON {pattern} FROM {account}";
				// ALL PRIVILEGES не включает право раздачи грантов
				if(MySqlGrants.HasGrantOption(grant))
					yield return $"REVOKE GRANT OPTION ON {pattern} FROM {account}";
			}

			string privileges = PrivilegesFor(access);
			if(privileges != null)
				yield return $"GRANT {privileges} ON `{MySqlEscape.Pattern(access.BaseName)}`.* TO {account}";
		}

		/// <summary>null - доступа нет</summary>
		private static string PrivilegesFor(DbUserBaseAccess access) {
			if(!access.HasAccess)
				return null;
			if(access.IsAdmin)
				return AllPrivileges;
			if(access.ReadOnly)
				return "SELECT, LOCK TABLES, SHOW VIEW";
			return "SELECT, INSERT, UPDATE, DELETE, EXECUTE, CREATE TEMPORARY TABLES, LOCK TABLES, SHOW VIEW";
		}

		private void ReflectAccessInBase(string login, DbUserBaseAccess access) {
			BaseUsers.Sync(access.BaseName, new BaseUserRow {
				Login = login,
				Name = access.Name,
				Email = access.Email,
				Admin = access.IsAdmin,
				Deactivated = !access.HasAccess
			}, access.HasAccess);
		}

		// в base_access метабазы - только если доступ пришёл оттуда и у базы известен её идентификатор
		private void ReflectAccessInMetadata(string login, DbUserBaseAccess access) {
			if(access.BaseId <= 0)
				return;

			ReflectInMetadata(m => {
				var target = m.Users.GetUserByLogin(login);
				if(target != null)
					m.Users.ChangeBaseAccess(new Entities.BaseAccessRow {
						BaseId = access.BaseId,
						HasAccess = access.HasAccess,
						Admin = access.IsAdmin,
						ReadOnly = access.ReadOnly
					}, target);
			}, "изменение доступа", login);
		}

		private Dictionary<string, List<string>> ReadGrantsByHost(string login) {
			var hosts = HostsOf(login).ToList();
			var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			string sql = string.Join(";", hosts
				.Select(host => $"SHOW GRANTS FOR {AccountOf(login, host)}"));
			try {
				using(var multi = connection.QueryMultiple(sql)) {
					foreach(var host in hosts)
						result[host] = multi.Read<string>().ToList();
				}
			}
			catch(MySqlException ex) {
				logger.Debug(ex, "Не удалось получить гранты пользователя {0}", login);
			}
			return result;
		}

		// строка mysql.user - её заполняет Dapper, поэтому свойства выглядят «неиспользуемыми»
		// сеттеры зовёт Dapper через рефлексию, поэтому "не используется" тут неверно
		[SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed",
			Justification = "Строку заполняет Dapper")]
		private sealed class MySqlUserRow {
			public string Login { get; set; }
			public string Host { get; set; }
			public string AccountLocked { get; set; }
			public string SuperPriv { get; set; }
			public string CreateUserPriv { get; set; }
		}

		private IReadOnlyList<string> HostsOf(string login) =>
			userHosts.TryGetValue(login, out var hosts) && hosts.Count > 0
				? (IReadOnlyList<string>)hosts
				: new[] { AnyHost };

		private static string AccountOf(string login, string host) =>
			$"'{MySqlHelper.EscapeString(login)}'@'{MySqlHelper.EscapeString(host)}'";

		private static string GrantAdmin(string account) =>
			$"GRANT {AllPrivileges} ON *.* TO {account} WITH GRANT OPTION";

		private static string RevokeAdmin(string account) =>
			$"REVOKE {AllPrivileges}, GRANT OPTION ON *.* FROM {account}";

		private static void ValidateLogin(string login) {
			if(string.IsNullOrWhiteSpace(login))
				throw new ArgumentException("Логин пользователя не может быть пустым", nameof(login));
			if(login.Length > 80)
				throw new ArgumentException("Логин пользователя длиннее 80 символов", nameof(login));
		}

		#endregion

		private void EnsureOpen() {
			if(connection.State != ConnectionState.Open)
				connection.Open();
		}

		public void Dispose() {
			connection?.Dispose();
		}
	}
}
