using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using QS.DbManagement.MariaDb;
using QS.DbManagement.MariaDb.QSLauncher;
using QS.DBScripts.Controllers;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace QS.DbManagement {
	public class MariaDBProvider : IDbProvider {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string MessageTitle = "Создание базы данных";
		private const string AnyHost = "%";
		private const string LocalHost = "localhost";

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
		public byte ProductCode { get; }
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, byte productCode, string password = null) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			Server = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			ProductCode = productCode;

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

		#region QSLauncher

		public bool CanRefreshMetadata => CanCreateDatabase;
		public bool CanBackupDatabase { get; } = true;

		private LauncherMetadataManagement metadata;
		/// <summary>false - собрать метабазу ещё не пробовали; true - пробовали, и повторять не будем</summary>
		private bool metadataMade;
		private bool serverLoggedIn;

		private LauncherMetadataManagement Metadata
		{
			get
			{
				// до логина права ещё не посчитаны - собранная сейчас метабаза запомнила бы их заниженными
				if(!serverLoggedIn)
					return null;

				// один раз не собралась - больше не пробуем
				if(metadataMade)
					return metadata;

				try
				{
					metadata = new LauncherMetadataManagement(
						new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString),
						CanCreateDatabase, UserName, ProductCode);
				}
				catch(Exception ex) when(
					ex is MySqlException //базы нет, нет прав, сервер недоступен
					|| ex is InvalidOperationException|| ex is KeyNotFoundException //схема не та, что можно читать
					|| ex is ArgumentException //текущего пользователя в метабазе нет
					)
				{
					logger.Debug(ex, "QSLauncher база недоступна, используем прямой доступ к серверу");
				}
				metadataMade = true;
				return metadata;
			}
		}

		private T FromMetadataOrDirect<T>(Func<LauncherMetadataManagement, T> fromMetadata, Func<T> direct)
		{
			var launcher = Metadata;
			if(launcher != null)
			{
				try
				{
					return fromMetadata(launcher);
				}
				catch(Exception ex)
				{
					logger.Debug(ex, "Не удалось прочитать из метабазы, работаем напрямую с сервером.");
				}
			}
			return direct();
		}

		// Синхронизация таблицы users без привязки к базе
		private BaseUsersManagement baseUsers;
		private BaseUsersManagement BaseUsers
			=> baseUsers ?? (baseUsers = new BaseUsersManagement(new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString)));

		public RefreshMetadataResponse RefreshMetadata()
		{
			var bases = RefreshBases();
			if(!bases.Success)
				return bases;

			var users = RefreshUsers();
			if(!users.Success)
				return users;

			return new RefreshMetadataResponse
			{
				Success = true,
				SyncedBases = bases.SyncedBases,
				SyncedUsers = users.SyncedUsers
			};
		}

		public RefreshMetadataResponse RefreshBases()
		{
			var launcher = Metadata;
			if(launcher == null)
				return MetadataUnavailable();

			return new RefreshMetadataResponse
			{
				Success = true,
				SyncedBases = launcher.Bases.SyncBases()
			};
		}

		public RefreshMetadataResponse RefreshUsers()
		{
			var launcher = Metadata;
			if(launcher == null)
				return MetadataUnavailable();

			var users = GetUsersDirect().Select(u => u.Login).ToList();
			return new RefreshMetadataResponse
			{
				Success = true,
				SyncedUsers = launcher.Users.SyncUsers(users)
			};
		}

		private static RefreshMetadataResponse MetadataUnavailable() =>
			new RefreshMetadataResponse
			{
				Success = false,
				ErrorMessage = $"База {LauncherMetadataManagement.LauncherBaseName} недоступна, "
					+ "синхронизировать метаинформацию некуда."
			};

		private void RegisterInLauncherMetadata(DbCreationRequest request)
		{
			try
			{
				Metadata?.CreateBaseWithCreatorAccess(new DbInfo { Title = request.DbTitle, BaseName = request.DbName });
			}
			catch(Exception ex)
			{
				logger.Warn(ex, "Не удалось зарегистрировать базу {0} в метабазе QSLauncher.", request.DbName);
			}
		}

		#region Отображение сущностей метабазы в публичные

		private DbUserInfo ToDbUserInfo(LauncherUserInfo u) => new DbUserInfo
		{
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

		private static LauncherUserInfo ToLauncherUser(DbUserInfo u) => new LauncherUserInfo
		{
			Login = u.Login,
			Name = u.Name,
			Email = u.Email,
			Phone = u.Phone,
			Post = u.Post,
			Comment = u.Comment,
			Disabled = u.Disabled,
			IsAccountAdmin = u.IsAdmin
		};

		private static DbUserBaseAccess ToDbUserBaseAccess(BaseAccessRow r) => new DbUserBaseAccess
		{
			BaseId = r.BaseId,
			BaseName = r.BaseName,
			Title = r.BaseTitle,
			HasAccess = r.HasAccess,
			IsAdmin = r.Admin,
			ReadOnly = r.ReadOnly
		};

		#endregion

		private void ReflectInMetadata(Action<LauncherMetadataManagement> action, string operation, string subject)
		{
			var launcher = Metadata;
			if(launcher == null)
				return;
			try
			{
				action(launcher);
			}
			catch(Exception ex)
			{
				logger.Warn(ex, "Не удалось отразить {0} ({1}) в метабазе.", operation, subject);
			}
		}

		private void ReflectUserUpdateInMetadata(DbUserInfo user, string newPassword)
		{
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

		public LoginToServerResponse LoginToServer() {
			try {
				var grants = OnConnection(c => c.Query<string>("SHOW GRANTS FOR CURRENT_USER").ToList());

				IsAdmin = MySqlGrants.HasGlobalAdmin(grants);
				CanManageBaseAccess = MySqlGrants.HasGlobalGrantOption(grants);

				var privileges = new HashSet<string>(grants
					.Where(g => MySqlGrants.Scope(g) != null)
					.SelectMany(MySqlGrants.Privileges), StringComparer.Ordinal);

				CanCreateDatabase = IsAdmin || HasPrivilege(privileges, "CREATE");
				CanDropDatabase = IsAdmin || HasPrivilege(privileges, "DROP");

				// права пересчитаны - метабазу, если её уже собирали, пересоберём с новыми
				metadata = null;
				metadataMade = false;
				serverLoggedIn = true;

				return new LoginToServerResponse {
					Success = true,
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
			=> privileges.Contains(MySqlAccess.AllPrivileges) || privileges.Contains(privilege);

		#region Управление базами

		public List<DbInfo> GetUserDatabases() {
			return FromMetadataOrDirect(
				metadata => metadata.Bases.GetBases(UserName).ToList(),
				() => GetUserDatabasesDirect());
		}

		private static readonly string[] DbInfoParameters = { "ProductCode", "BaseTitle", "version" };

		private List<DbInfo> GetUserDatabasesDirect() {
			var names = OnConnection(c => c.Query<string>("SHOW DATABASES").ToList())
				.Except(MySqlSystemObjects.Databases, StringComparer.OrdinalIgnoreCase)
				.ToList();

			// одним запросом по серверному соединению: подключение к каждой базе - это ещё один пул
			var parameters = OnConnection(c => BaseParametersReader.ReadMany(c, names, DbInfoParameters));

			return names
				.Select(dbName => ToDbInfo(dbName, parameters))
				.Where(db => db != null)
				.ToList();
		}

		/// <summary>null - база не от нашего продукта либо её параметры прочитать не вышло</summary>
		private DbInfo ToDbInfo(string dbName, IReadOnlyDictionary<string, Dictionary<string, string>> byDatabase) {
			if(!byDatabase.TryGetValue(dbName, out var parameters))
				return null;

			if(!parameters.TryGetValue("ProductCode", out var code) || !byte.TryParse(code, out var baseProduct)
				|| baseProduct != ProductCode)
				return null;

			return new DbInfo {
				BaseName = dbName,
				Title = parameters.TryGetValue("BaseTitle", out var title) ? title : dbName,
				Version = parameters.TryGetValue("version", out var version) ? version : null
			};
		}

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			try {
				var toDatabase = new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString) {
					Database = dbInfo.BaseName
				};

				return new LoginToDatabaseResponse {
					Success = true,
					ConnectionString = toDatabase.ConnectionString,
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
					if(!DropDatabase(new DbInfo { BaseName = request.DbName })) {
						request.Interaction.ReportError("Не удалось удалить существующую базу: " + request.DbName, MessageTitle);
						return false;
					}
					break;
				case ToDoWithExistingDatabase.Rewrite:
					// базу сносимp
					// записи метабазы и выданные на базу права должны пережить перезапись
					OnConnection(c => c.Execute($"DROP DATABASE IF EXISTS `{MySqlEscape.Identifier(request.DbName)}`"));
					break;
				default: // Nothing
					return false;
			}

			CreateEmptyDatabase(request.DbName);
			return true;
		}

		private void CreateEmptyDatabase(string dbName) =>
			OnConnection(c => c.Execute($"CREATE DATABASE `{MySqlEscape.Identifier(dbName)}`"));

		private bool DoesDataBaseExist(string dbName) =>
			OnConnection(c => c.ExecuteScalar<int>(
				"SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @name;",
				new { name = dbName })) > 0;

		public bool DropDatabase(DbInfo database) {
			OnConnection(c => c.Execute($"DROP DATABASE IF EXISTS `{MySqlEscape.Identifier(database.BaseName)}`"));
			CleanDatabasePrivileges(database.BaseName);

			ReflectInMetadata(m => m.Bases.SyncWithDelete(database), "удаление базы", database.BaseName);

			return true;
		}

		private void CleanDatabasePrivileges(string dbName) {
			var names = new[] { dbName, dbName.Replace("_", "\\_").Replace("%", "\\%") }
				.Distinct(StringComparer.Ordinal).ToArray();
			try {
				OnConnection(c => c.Execute(
					"DELETE FROM mysql.db WHERE Db IN @names;" +
					"DELETE FROM mysql.tables_priv WHERE Db IN @names;" +
					"DELETE FROM mysql.columns_priv WHERE Db IN @names;" +
					"DELETE FROM mysql.procs_priv WHERE Db IN @names;" +
					"FLUSH PRIVILEGES;",
					new { names }));
			}
			catch(MySqlException ex) {
				// у текущего пользователя может не быть прав на mysql.*
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

		private static readonly string[] NewUserHosts = { AnyHost, LocalHost };

		private readonly Dictionary<string, List<string>> userHosts = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		private enum AccountLockStorage {
			/// <summary>Сервер блокировку не поддерживает</summary>
			Unsupported,
			/// <summary>колонка mysql.user.account_locked</summary>
			UserColumn,
			/// <summary>JSON в mysql.global_priv, в mysql.user такой колонки нет</summary>
			GlobalPriv
		}

		private AccountLockStorage? accountLockStorage;
		private AccountLockStorage AccountLocks {
			get {
				if(accountLockStorage == null)
					accountLockStorage = DetectAccountLockStorage();
				return accountLockStorage.Value;
			}
		}

		private AccountLockStorage DetectAccountLockStorage() {
			bool hasUserColumn = OnConnection(c => c.ExecuteScalar<long>(
				"SELECT COUNT(*) FROM information_schema.COLUMNS " +
				"WHERE TABLE_SCHEMA = 'mysql' AND TABLE_NAME = 'user' AND COLUMN_NAME = 'account_locked'")) > 0;
			if(hasUserColumn)
				return AccountLockStorage.UserColumn;

			bool hasGlobalPriv = OnConnection(c => c.ExecuteScalar<long>(
				"SELECT COUNT(*) FROM information_schema.TABLES " +
				"WHERE TABLE_SCHEMA = 'mysql' AND TABLE_NAME = 'global_priv'")) > 0;

			return hasGlobalPriv ? AccountLockStorage.GlobalPriv : AccountLockStorage.Unsupported;
		}

		private bool SupportsAccountLock => AccountLocks != AccountLockStorage.Unsupported;
		public bool CanChangeOwnPassword { get; } = true;

		public bool ChangeOwnPassword(string newPassword) {
			if(string.IsNullOrEmpty(newPassword))
				throw new ArgumentException("Пароль не может быть пустым", nameof(newPassword));
			OnConnection(c => c.Execute($"SET PASSWORD = PASSWORD('{MySqlHelper.EscapeString(newPassword)}')"));

			ConnectionStringBuilder.Password = newPassword;

			ReflectInMetadata(m => m.Users.SyncWithChangeOwnPassword(UserName, newPassword), "смену пароля", UserName);
			return true;
		}

		public List<DbUserInfo> GetUsers() {
			return FromMetadataOrDirect(
				metadata => metadata.Users.GetUsers().Select(ToDbUserInfo).ToList(),
				GetUsersDirect);
		}

		private List<MySqlUserRow> ReadServerAccounts()
			=> OnConnection(c => c.Query<MySqlUserRow>(ServerAccountsQuery()).ToList());

		// один логин заведён на сервере под несколькими хостами - админ он, только если админ везде
		private static bool IsAccountAdmin(IEnumerable<MySqlUserRow> accounts)
			=> accounts.All(r => MySqlGrants.IsGlobalAdmin(IsYes(r.SuperPriv), IsYes(r.CreateUserPriv)));

		private List<DbUserInfo> GetUsersDirect() {
			// чтение и пересборка кеша хостов - одна операция: между ними словарь пуст,
			// и параллельный HostsOf увидел бы учётку без хостов
			lock(serverLock) {
				var rows = ReadServerAccounts();

				userHosts.Clear();
				var result = new List<DbUserInfo>();
				foreach(var accounts in rows.Where(IsRealUser).GroupBy(r => r.Login, StringComparer.Ordinal)) {
					userHosts[accounts.Key] = accounts.Select(r => string.IsNullOrEmpty(r.Host) ? AnyHost : r.Host).ToList();
					result.Add(new DbUserInfo {
						Login = accounts.Key,
						Disabled = accounts.All(r => IsYes(r.AccountLocked)),
						IsAdmin = IsAccountAdmin(accounts),
						IsCurrentUser = string.Equals(accounts.Key, UserName, StringComparison.OrdinalIgnoreCase)
					});
				}
				return result;
			}
		}

		/// <summary>
		/// Учётки сервера с признаком блокировки. У MariaDB флаг лежит JSON-ом в mysql.global_priv
		/// у MySQL - колонкой в mysql.user
		/// в обоих случаях приводим к 'Y'/'N', как ждёт MySqlUserRow
		/// </summary>
		private string ServerAccountsQuery() {
			string locked;
			string join = string.Empty;

			switch(AccountLocks) {
				case AccountLockStorage.UserColumn:
					locked = "u.account_locked";
					break;
				case AccountLockStorage.GlobalPriv:
					locked = "IF(JSON_VALUE(p.Priv, '$.account_locked') = 1, 'Y', 'N')";
					join = "LEFT JOIN mysql.global_priv p ON p.User = u.User AND p.Host = u.Host";
					break;
				default:
					locked = "NULL";
					break;
			}

			return $"SELECT u.User AS Login, u.Host, {locked} AS AccountLocked, " +
				"u.Super_priv AS SuperPriv, u.Create_user_priv AS CreateUserPriv " +
				$"FROM mysql.user u {join}";
		}

		// служебные учётки самого сервера пользователю приложения не показываем
		private static bool IsRealUser(MySqlUserRow row) =>
			!string.IsNullOrEmpty(row.Login)
			&& !row.Login.StartsWith("mysql.", StringComparison.OrdinalIgnoreCase)
			&& !MySqlSystemObjects.Users.Contains(row.Login, StringComparer.OrdinalIgnoreCase);

		private static bool IsYes(string flag) => string.Equals(flag, "Y", StringComparison.OrdinalIgnoreCase);

		public bool CreateUser(DbUserInfo user, string password) {
			ValidateLogin(user?.Login);
			if(string.IsNullOrEmpty(password))
				throw new ArgumentException("Пароль не может быть пустым", nameof(password));

			// без чтения метабазы новый пользователь не увидит список баз
			var launcherAccess = new DbUserBaseAccess {
				BaseName = LauncherMetadataManagement.LauncherBaseName, ReadOnly = true, HasAccess = true
			};

			var statements = NewUserHosts
				.SelectMany(host => NewUserStatements(user, host, password, launcherAccess))
				.ToList();

			OnConnection(c => c.Execute(string.Join(";", statements)));
			lock(serverLock)
				userHosts[user.Login] = NewUserHosts.ToList();

			ReflectInMetadata(m => m.Users.CreateUser(ToLauncherUser(user), password), "создание", user.Login);
			return true;
		}

		private IEnumerable<string> NewUserStatements(DbUserInfo user, string host, string password,
			DbUserBaseAccess launcherAccess) {
			string account = MySqlAccess.UserOf(user.Login, host);
			string lockOption = user.Disabled && SupportsAccountLock ? " ACCOUNT LOCK" : string.Empty;

			yield return $"CREATE USER {account} IDENTIFIED BY '{MySqlHelper.EscapeString(password)}'{lockOption}";

			if(SupportsAdminFlag && user.IsAdmin)
				yield return MySqlAccess.GrantAdmin(account);

			foreach(var statement in MySqlAccess.Statements(user.Login, host, null, launcherAccess))
				yield return statement;
		}

		public bool UpdateUser(DbUserInfo user, string newPassword = null) {
			ValidateLogin(user?.Login);

			bool isAdminNow = SupportsAdminFlag
				&& MySqlGrants.HasGlobalAdmin(ReadGrantsByHost(user.Login).Values.SelectMany(g => g));

			var statements = HostsOf(user.Login)
				.SelectMany(host => UserChangeStatements(user, host, newPassword, isAdminNow))
				.ToList();

			if(statements.Any())
				OnConnection(c => c.Execute(string.Join(";", statements)));

			ReflectUserUpdateInMetadata(user, newPassword);
			ReflectProfileInBases(user);
			return true;
		}

		/// <summary>
		/// Профиль пишется сам по себе, а не попутно с выдачей доступа: правка одного имени
		/// должна доходить до баз, даже если доступы не трогали. Строку не заводим - обновляем
		/// там, где пользователь уже есть, остальные базы UPDATE просто не заденет
		/// </summary>
		private void ReflectProfileInBases(DbUserInfo user) {
			if(string.IsNullOrEmpty(user.Name) && string.IsNullOrEmpty(user.Email))
				return;

			BaseUsers.SyncProfile(GetUserDatabases().Select(db => db.BaseName), user.Login, user.Name, user.Email);
		}

		private IEnumerable<string> UserChangeStatements(DbUserInfo user, string host, string newPassword, bool isAdminNow) {
			string account = MySqlAccess.UserOf(user.Login, host);

			string options = string.Join(" ", AlterUserOptions(user, newPassword));
			if(options.Length > 0)
				yield return $"ALTER USER {account} {options}";

			if(SupportsAdminFlag && user.IsAdmin != isAdminNow)
				yield return user.IsAdmin ? MySqlAccess.GrantAdmin(account) : MySqlAccess.RevokeAdmin(account);
		}

		// через ALTER USER меняются только пароль и блокировка учётки
		private IEnumerable<string> AlterUserOptions(DbUserInfo user, string newPassword) {
			if(!string.IsNullOrEmpty(newPassword))
				yield return $"IDENTIFIED BY '{MySqlHelper.EscapeString(newPassword)}'";
			if(SupportsAccountLock)
				yield return user.Disabled ? "ACCOUNT LOCK" : "ACCOUNT UNLOCK";
		}

		public bool DeleteUser(string login) {
			ValidateLogin(login);

			var userBases = GetUserBaseAccess(login).ConvertAll(a => a.BaseName);

			OnConnection(c => c.Execute(string.Join(";", HostsOf(login)
				.Select(host => $"DROP USER IF EXISTS {MySqlAccess.UserOf(login, host)}"))));
			lock(serverLock)
				userHosts.Remove(login);

			ReflectInMetadata(m => {
				var target = m.Users.GetUserByLogin(login);
				if(target != null)
					m.Users.DeleteUser(target);
			}, "удаление", login);

			BaseUsers.SyncWithDeletingUser(login, userBases);

			return true;
		}

		public List<DbUserBaseAccess> GetUserBaseAccess(string login) {
			return FromMetadataOrDirect(
				metadata => {
					var user = metadata.Users.GetUserByLogin(login)
						?? throw new InvalidOperationException($"Пользователь {login} не найден в метабазе.");
					var list = metadata.Users.GetUserBaseAccess(user).Select(ToDbUserBaseAccess).ToList();
					FillUsersProfiles(list, login);
					return list;
				},
				() => GetUserBaseAccessDirect(login));
		}

		private List<DbUserBaseAccess> GetUserBaseAccessDirect(string login) {
			var grants = ReadGrantsByHost(login).Values.SelectMany(g => g).ToList();
			bool globalAdmin = MySqlGrants.HasGlobalAdmin(grants);

			var result = GetUserDatabasesDirect()
				.Select(db => globalAdmin ? MySqlAccess.FullAccessByGlobalGrant(db) : MySqlAccess.FromGrants(db, grants))
				.ToList();

			FillUsersProfiles(result, login);
			return result;
		}

		private void FillUsersProfiles(IEnumerable<DbUserBaseAccess> accesses, string login) {
			var withAccess = accesses.Where(a => a.HasAccess && !string.IsNullOrEmpty(a.BaseName)).ToList();
			if(!withAccess.Any())
				return;

			// один запрос на все базы: поштучно это два запроса на каждую
			var profiles = BaseUsers.TryGetProfiles(withAccess.Select(a => a.BaseName), login);

			foreach(var access in withAccess) {
				if(!profiles.TryGetValue(access.BaseName, out var profile))
					continue;
				access.Name = profile.Name;
				access.Email = profile.Email;
			}
		}

		public bool SetUserBaseAccess(string login, DbUserBaseAccess access) {
			ValidateLogin(login);
			if(string.IsNullOrWhiteSpace(access?.BaseName))
				throw new ArgumentException("Не указано имя базы", nameof(access));

			var grantsByHost = ReadGrantsByHost(login);
			if(!grantsByHost.Any())
				throw new InvalidOperationException($"Пользователь {login} не найден на сервере.");

			if(MySqlGrants.HasGlobalAdmin(grantsByHost.Values.SelectMany(g => g)))
				throw new InvalidOperationException(
					$"У пользователя {login} глобальные права на весь сервер");

			var statements = grantsByHost
				.SelectMany(hostGrants => MySqlAccess.Statements(login, hostGrants.Key, hostGrants.Value, access))
				.ToList();
			if(statements.Any())
				OnConnection(c => c.Execute(string.Join(";", statements)));

			ReflectInMetadata(m => {
				var target = m.Users.GetUserByLogin(login);
				if(target != null)
					m.Users.ChangeBaseAccess(
						new BaseAccessRow { BaseId = access.BaseId, BaseName = access.BaseName, HasAccess = access.HasAccess, Admin = access.IsAdmin, ReadOnly = access.ReadOnly
						}, target);
			}, "изменение доступа", login);
			BaseUsers.SyncWithUserTable(access.BaseName, new BaseUserRow { Admin = access.IsAdmin, Deactivated = !access.HasAccess, Email = access.Email, Login = login, Name = access.Name}, access.HasAccess);

			return true;
		}

		private Dictionary<string, List<string>> ReadGrantsByHost(string login)
		{
			var hosts = HostsOf(login).ToList();
			var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			string sql = string.Join(";", hosts
				.Select(host => $"SHOW GRANTS FOR {MySqlAccess.UserOf(login, host)}"));
			try {
				OnConnection(c => {
					using(var multi = c.QueryMultiple(sql)) {
						foreach(var host in hosts)
							result[host] = multi.Read<string>().ToList();
					}
				});
			}
			catch(MySqlException ex) {
				logger.Debug(ex, "Не удалось получить гранты пользователя {0}", login);
			}
			return result;
		}

		private sealed class MySqlUserRow {
			public string Login { get; set; }
			public string Host { get; set; }
			public string AccountLocked { get; set; }
			public string SuperPriv { get; set; }
			public string CreateUserPriv { get; set; }
		}

		private IReadOnlyList<string> HostsOf(string login)
		{
			lock(serverLock) {
				if(!userHosts.ContainsKey(login)) {
					try {
						GetUsersDirect();
					}
					catch(MySqlException ex) {
						logger.Debug(ex, "Не удалось прочитать хосты учётной записи {0}", login);
					}
				}

				return userHosts.TryGetValue(login, out var hosts) && hosts.Any()
					? (IReadOnlyList<string>)hosts.ToList()
					: NewUserHosts;
			}
		}

		private static void ValidateLogin(string login)
		{
			if(string.IsNullOrWhiteSpace(login))
				throw new ArgumentException("Логин пользователя не может быть пустым", nameof(login));
			if(login.Length > 80)
				throw new ArgumentException("Логин пользователя длиннее 80 символов", nameof(login));
		}

		#endregion

		private readonly object serverLock = new object();

		private T OnConnection<T>(Func<MySqlConnection, T> query) {
			lock(serverLock) {
				EnsureOpen();
				return query(connection);
			}
		}

		private void OnConnection(Action<MySqlConnection> command) {
			lock(serverLock) {
				EnsureOpen();
				command(connection);
			}
		}

		private void EnsureOpen()
		{
			if(connection.State != ConnectionState.Open)
				connection.Open();
		}

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if(disposed)
				return;

			if(disposing)
				connection?.Dispose();

			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
