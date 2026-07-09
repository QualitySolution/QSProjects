using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace QS.DbManagement
{
	public class MariaDBProvider : IDbProvider {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };

		readonly MySqlConnection connection;

		/// <summary>
		/// Публичный - в типе родключения нужен доступ, реализацию он знает и так
		/// </summary>
		public MySqlConnectionStringBuilder ConnectionStringBuilder { get; }

		public bool IsConnected => connection.State == ConnectionState.Open;

		public bool IsAdmin { get; private set; }

		public bool CanCreateDatabase { get; private set; }
		public bool CanDropDatabase { get; private set; }
		public bool CanManageBaseAccess { get; private set; }

		#region Параметры подключения
		public string Server { get; }
		public string UserName { get; }
		private readonly string password;
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			string serverValue = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			this.password = password;

			string host = serverValue;
			uint? port = null;
			if(serverValue.Contains(":")) {
				var parts = serverValue.Split(':');
				host = parts[0];
				if(uint.TryParse(parts[1], out var parsedPort))
					port = parsedPort;
			}
			Server = serverValue;

			ConnectionStringBuilder = new MySqlConnectionStringBuilder {
				Server = host,
				UserID = UserName,
				Password = password,
				AllowUserVariables = true
			};
			if(port != null)
				ConnectionStringBuilder.Port = port.Value;
			connection = new MySqlConnection(ConnectionStringBuilder.ConnectionString);
		}

		#region Управление базами

		public LoginToServerResponse LoginToServer() {
			try {
				EnsureOpen();

				var grants = connection.Query<string>("SHOW GRANTS FOR CURRENT_USER").ToList();

				IsAdmin = HasGlobalAdminGrant(grants);
				CanManageBaseAccess = HasGlobalGrantOption(grants);

				var privileges = new HashSet<string>(grants
					.Where(g => GrantScope(g) != null)
					.SelectMany(GrantPrivileges));

				CanCreateDatabase = IsAdmin || privileges.Contains("ALL PRIVILEGES") || privileges.Contains("CREATE");
				CanDropDatabase = IsAdmin || privileges.Contains("ALL PRIVILEGES") || privileges.Contains("DROP");

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

		public List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo) {
			var result = new List<DbInfo>();

			EnsureOpen();

			var databases = connection.Query<string>("SHOW DATABASES").ToList();
			byte expectedProductCode = applicationInfo.ProductCode;

			foreach(var dbName in databases.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase)) {
				byte? productCode = null;
				string version = null;
				string title = null;
				try {
					var rows = connection.Query<(string name, string str_value)>(
						$"SELECT name, str_value FROM `{dbName}`.base_parameters WHERE name IN ('ProductCode', 'version', 'BaseTitle')").ToList();
					foreach(var row in rows) {
						if(string.Equals(row.name, "ProductCode", StringComparison.OrdinalIgnoreCase))
							productCode = Convert.ToByte(row.str_value);
						else if(string.Equals(row.name, "version", StringComparison.OrdinalIgnoreCase))
							version = row.str_value;
						else if(string.Equals(row.name, "BaseTitle", StringComparison.OrdinalIgnoreCase))
							title = row.str_value;
					}
				}
				catch(MySqlException ex) {
					logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
					continue;
				}

				if((expectedProductCode != null)
					&& (productCode != expectedProductCode))
					continue;

				result.Add(new DbInfo {
					BaseName = dbName,
					Title = title ?? dbName,
					Version = version
				});
			}

			return result;
		}

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			try {
				ConnectionStringBuilder.Database = dbInfo.BaseName;

				return new LoginToDatabaseResponse {
					Success = true,
					ConnectionString = ConnectionStringBuilder.ConnectionString,
					Login = UserName,
					Parameters = new Dictionary<string, string> {
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

		public bool CreateDatabase(DbCreationRequest request)
		{
			EnsureOpen();

			if(request == null)
				throw new ArgumentNullException(nameof(request));
			connection.Execute($"CREATE DATABASE IF NOT EXISTS `{request.DbName}`");

			var connectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString) {
				Database = request.DbName
			};
			return request.CreationModel.RunCreation(
				connectionStringBuilder.ConnectionString,
				request.DbName, request.DbTitle,
				request.Progress, request.CancellationToken);
		}

		/// <summary>
		/// Создание базы с наполнением из пользовательского дампа
		/// Метод блокирующий - вызывать из фонового потока
		/// </summary>
		public bool ImportDatabase(DbImportRequest request)
		{
			EnsureOpen();

			if(request == null)
				throw new ArgumentNullException(nameof(request));
			connection.Execute($"CREATE DATABASE IF NOT EXISTS `{request.DbName}`");

			request.DumpService.Import(
				ConnectionStringBuilder.ConnectionString, request.DbName, request.DumpFilePath,
				request.Progress, request.CancellationToken, request.DbTitle);
			request.CancellationToken.ThrowIfCancellationRequested();
			return true;
		}

		public bool DropDatabase(DbInfo database, IApplicationInfo applicationInfo)
		{
			EnsureOpen();

			string sql = $"DROP DATABASE IF EXISTS `{database.BaseName}`";
			return connection.Execute(sql) != 0;
		}

		/// <summary>
		/// Резервное копирование базы в скрипт
		/// Метод блокирующий - вызывать из фонового потока
		/// </summary>
		public void BackupDatabase(DbInfo database, string filePath, IDbDumpService dumpService, IProgressBarDisplayable progress, CancellationToken cancellation) {
			dumpService.Export(ConnectionStringBuilder.ConnectionString, database.BaseName, filePath, progress, cancellation);
		}

		#region Управление пользователями

		public DbUserFields SupportedUserFields =>
			DbUserFields.BaseReadOnly
			| (CanManageUsers && SupportsAccountLock ? DbUserFields.Disabling : DbUserFields.None);

		public bool CanManageUsers => IsAdmin;

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

			connection.Execute($"ALTER USER CURRENT_USER() IDENTIFIED BY '{EscapeString(newPassword)}'");
			return true;
		}

		public List<DbUserInfo> GetUsers() {
			EnsureOpen();

			string lockedColumn = SupportsAccountLock ? "account_locked" : "NULL";
			var rows = connection.Query<MySqlUserRow>(
				$"SELECT User AS Login, Host, {lockedColumn} AS AccountLocked FROM mysql.user ORDER BY User, Host").ToList();

			userHosts.Clear();
			var result = new List<DbUserInfo>();
			foreach(var userRows in rows
				.Where(r => !string.IsNullOrEmpty(r.Login)
					&& !r.Login.StartsWith("mysql.", StringComparison.OrdinalIgnoreCase)
					&& !SystemUsers.Contains(r.Login, StringComparer.OrdinalIgnoreCase))
				.GroupBy(r => r.Login, StringComparer.Ordinal)) {

				userHosts[userRows.Key] = userRows.Select(r => string.IsNullOrEmpty(r.Host) ? "%" : r.Host).ToList();
				result.Add(new DbUserInfo {
					Login = userRows.Key,
					// отключён, только если заблокированы все хосты логина
					Disabled = userRows.All(r => string.Equals(r.AccountLocked, "Y", StringComparison.OrdinalIgnoreCase)),
					IsCurrentUser = string.Equals(userRows.Key, UserName, StringComparison.OrdinalIgnoreCase)
				});
			}
			return result;
		}

		public bool CreateUser(DbUserInfo user, string password) {
			ValidateLogin(user?.Login);
			if(string.IsNullOrEmpty(password))
				throw new ArgumentException("Пароль не может быть пустым", nameof(password));
			EnsureOpen();

			string lockOption = user.Disabled && SupportsAccountLock ? " ACCOUNT LOCK" : string.Empty;
			connection.Execute($"CREATE USER '{EscapeString(user.Login)}'@'%' IDENTIFIED BY '{EscapeString(password)}'{lockOption}");
			userHosts[user.Login] = new List<string> { "%" };
			return true;
		}

		public bool UpdateUser(DbUserInfo user, string newPassword = null) {
			ValidateLogin(user?.Login);
			EnsureOpen();

			var options = new List<string>();
			if(!string.IsNullOrEmpty(newPassword))
				options.Add($"IDENTIFIED BY '{EscapeString(newPassword)}'");
			if(SupportsAccountLock)
				options.Add(user.Disabled ? "ACCOUNT LOCK" : "ACCOUNT UNLOCK");
			if(options.Count == 0)
				return true;

			// одним батчем по всем хостам логина - меньше сетевых обращений (роллбека тут всё равно нет: DDL по учёткам самокоммитится)
			string suffix = string.Join(" ", options);
			connection.Execute(string.Join(";", HostsOf(user.Login)
				.Select(host => $"ALTER USER '{EscapeString(user.Login)}'@'{EscapeString(host)}' {suffix}")));
			return true;
		}

		public bool DeleteUser(string login) {
			ValidateLogin(login);
			EnsureOpen();

			connection.Execute(string.Join(";", HostsOf(login)
				.Select(host => $"DROP USER IF EXISTS '{EscapeString(login)}'@'{EscapeString(host)}'")));
			userHosts.Remove(login);
			return true;
		}

		public List<DbUserBaseAccess> GetUserBaseAccess(string login, IApplicationInfo applicationInfo) {
			EnsureOpen();

			var databases = GetUserDatabases(applicationInfo);
			var grants = ReadGrantsByHost(login).Values.SelectMany(g => g).ToList();

			bool globalAdmin = HasGlobalAdminGrant(grants);

			return databases.Select(db => {
				var access = new DbUserBaseAccess { BaseName = db.BaseName, Title = db.Title };
				if(globalAdmin) {
					// доступ следует из грантов на *.* - аддитивная модель прав не позволяет
					// сузить его точечным REVOKE, поэтому строки не редактируются
					access.HasAccess = true;
					access.IsAdmin = true;
					access.CanEdit = false;
					return access;
				}

				var privileges = grants
					.Where(g => {
						var scope = GrantScope(g);
						if(scope == null)
							return false;
						// шаблонные гранты вида не разворачиваем
						return scope == "*" || string.Equals(UnescapeGrantPattern(scope), db.BaseName, StringComparison.OrdinalIgnoreCase);
					})
					.SelectMany(GrantPrivileges)
					.Where(p => p != "USAGE")
					.ToList();

				if(privileges.Count == 0)
					return access;

				access.HasAccess = true;
				if(privileges.Contains("ALL PRIVILEGES"))
					access.IsAdmin = true;
				else if(privileges.All(p => p == "SELECT" || p == "LOCK TABLES" || p == "SHOW VIEW"))
					access.ReadOnly = true;
				return access;
			}).ToList();
		}

		public bool SetUserBaseAccess(string login, DbUserBaseAccess access, IApplicationInfo applicationInfo) {
			ValidateLogin(login);
			if(string.IsNullOrWhiteSpace(access?.BaseName))
				throw new ArgumentException("Не указано имя базы", nameof(access));
			EnsureOpen();

			var grantsByHost = ReadGrantsByHost(login);
			if(grantsByHost.Count == 0)
				throw new InvalidOperationException($"Пользователь {login} не найден на сервере.");

			if(HasGlobalAdminGrant(grantsByHost.Values.SelectMany(g => g)))
				throw new InvalidOperationException(
					$"У пользователя {login} глобальные права на весь сервер");

			string privileges = null;
			if(access.HasAccess) {
				if(access.IsAdmin)
					privileges = "ALL PRIVILEGES";
				else if(access.ReadOnly)
					privileges = "SELECT, LOCK TABLES, SHOW VIEW";
				else
					privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, CREATE TEMPORARY TABLES, LOCK TABLES, SHOW VIEW";
			}

			var statements = new List<string>();
			foreach(var hostGrants in grantsByHost) {
				string user = $"'{EscapeString(login)}'@'{EscapeString(hostGrants.Key)}'";

				foreach(var grant in hostGrants.Value) {
					string scope = GrantScope(grant);
					if(scope == null || scope == "*"
						|| !string.Equals(UnescapeGrantPattern(scope), access.BaseName, StringComparison.OrdinalIgnoreCase))
						continue;
					string pattern = $"`{EscapeIdentifier(scope)}`.*";
					if(GrantPrivileges(grant).Any(p => p != "USAGE"))
						statements.Add($"REVOKE ALL PRIVILEGES ON {pattern} FROM {user}");
					// ALL PRIVILEGES не включает право раздачи грантов - его отзываем отдельно
					if(grant.IndexOf("WITH GRANT OPTION", StringComparison.OrdinalIgnoreCase) >= 0)
						statements.Add($"REVOKE GRANT OPTION ON {pattern} FROM {user}");
				}

				if(privileges != null)
					statements.Add($"GRANT {privileges} ON `{EscapeGrantPattern(access.BaseName)}`.* TO {user}");
			}

			if(statements.Count > 0)
				connection.Execute(string.Join(";", statements));
			return true;
		}

		private Dictionary<string, List<string>> ReadGrantsByHost(string login) {
			var hosts = HostsOf(login).ToList();
			var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			string sql = string.Join(";", hosts
				.Select(h => $"SHOW GRANTS FOR '{EscapeString(login)}'@'{EscapeString(h)}'"));
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

		private class MySqlUserRow {
			public string Login { get; set; }
			public string Host { get; set; }
			public string AccountLocked { get; set; }
		}

		private IReadOnlyList<string> HostsOf(string login) =>
			userHosts.TryGetValue(login, out var hosts) && hosts.Count > 0
				? (IReadOnlyList<string>)hosts
				: new[] { "%" };

		private static void ValidateLogin(string login) {
			if(string.IsNullOrWhiteSpace(login))
				throw new ArgumentException("Логин пользователя не может быть пустым");
			if(login.Length > 80)
				throw new ArgumentException("Логин пользователя длиннее 80 символов");
		}

		private static bool HasGlobalAdminGrant(IEnumerable<string> grants) =>
			grants.Any(g => {
				if(GrantScope(g) != "*")
					return false;
				var privileges = GrantPrivileges(g).ToList();
				return privileges.Contains("ALL PRIVILEGES")
					|| privileges.Contains("SUPER")
					|| privileges.Contains("CREATE USER");
			});

		private static bool HasGlobalGrantOption(IEnumerable<string> grants) =>
			grants.Any(g => GrantScope(g) == "*"
				&& g.Contains("WITH GRANT OPTION"));

		private static string EscapeString(string value) =>
			value == null ? string.Empty : value.Replace("\\", "\\\\").Replace("'", "\\'");

		private static string EscapeIdentifier(string value) =>
			value == null ? string.Empty : value.Replace("`", "``");

		private static string EscapeGrantPattern(string dbName) =>
			EscapeIdentifier(dbName).Replace("_", "\\_").Replace("%", "\\%");

		private static string UnescapeGrantPattern(string pattern) =>
			pattern.Replace("\\_", "_").Replace("\\%", "%");

		private static string GrantScope(string grant) {
			int onIdx = grant.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
			if(onIdx < 0)
				return null;
			string rest = grant.Substring(onIdx + 4).TrimStart(); //4 = " ON "

			string scope;
			int pos;
			if(rest.StartsWith("`", StringComparison.Ordinal)) {
				var name = new StringBuilder();
				pos = 1;
				while(pos < rest.Length) {
					if(rest[pos] == '`') {
						if(pos + 1 < rest.Length && rest[pos + 1] == '`') {
							name.Append('`');
							pos += 2;
							continue;
						}
						pos++;
						break;
					}
					name.Append(rest[pos]);
					pos++;
				}
				scope = name.ToString();
			}
			else {
				pos = rest.IndexOf('.');
				if(pos < 0)
					return null;
				scope = rest.Substring(0, pos).Trim();
			}

			if(pos + 1 >= rest.Length || rest[pos] != '.' || rest[pos + 1] != '*')
				return null;
			return scope;
		}

		/// <summary>Список привилегий из строки GRANT</summary>
		private static IEnumerable<string> GrantPrivileges(string grant) {
			int grantIdx = grant.IndexOf("GRANT ", StringComparison.OrdinalIgnoreCase);
			int onIdx = grant.IndexOf(" ON ", StringComparison.OrdinalIgnoreCase);
			if(grantIdx < 0 || onIdx < 0 || onIdx <= grantIdx)
				return Enumerable.Empty<string>();
			int start = grantIdx + 6; //6 = "GRANT "
			string privsPart = grant.Substring(start, onIdx - start);
			privsPart = Regex.Replace(privsPart, @"\([^)]*\)", string.Empty);
			return privsPart.Split(',')
				.Select(p => p.Trim().ToUpperInvariant())
				.Where(p => p.Length > 0);
		}

		#endregion

		private void EnsureOpen() {
			if(connection.State != ConnectionState.Open)
				connection.Open();
		}

		public void Dispose() {
			connection?.Dispose();
		}
		#endregion
	}
}
