using Dapper;
using MySqlConnector;
using QS.Cloud;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherUsersManagement
	{
		private const string LauncherBaseName = LauncherMetadataManagement.LauncherBaseName;
		private const string UsersTable = "server_users";
		private const int ER_DUP_ENTRY = 1062;

		private const string UserNotFound = "Пользователь с указанным именем не найден";
		private const string LoginTaken = "Такое имя пользователя уже занято";

		private static readonly HashSet<string> StructuralUserColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{ "id", "login", "password", "account_id" };

		private readonly string connectionString;
		private readonly byte productId;
		private readonly LauncherUserInfo userInfo;
		private bool? isAdminCached;

		public LauncherUsersManagement(MySqlConnectionStringBuilder connectionBuilder, string login, byte productId) {
			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;
			this.productId = productId;

			userInfo = GetLauncherUserInfo(login)
				?? throw new ArgumentException("Не удалось получить информацию о текущем пользователе", nameof(login));
		}

		public int CurrentAccountId => userInfo.AccountId;

		public IEnumerable<LauncherUserInfo> GetUsers() {
			RequireAdminFor("просмотра пользователей");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var columns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, UsersTable);
				string select = LauncherColumnMapper.SelectList(columns, typeof(LauncherUserInfo));
				string query = $"SELECT {select} FROM `{UsersTable}` WHERE account_id = @accountId AND product_id = @productId ORDER BY login;";
				return connection.Query<LauncherUserInfo>(query, new { accountId = userInfo.AccountId, productId = productId }).ToList();
			}
		}

		public LauncherUserInfo GetUserByLogin(string login) {
			RequireAdminFor("просмотра пользователя");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var columns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, UsersTable);
				string select = LauncherColumnMapper.SelectList(columns, typeof(LauncherUserInfo));
				string query = $"SELECT {select} FROM `{UsersTable}` WHERE login = @login AND account_id = @accountId AND product_id = @productId;";
				return connection.QueryFirstOrDefault<LauncherUserInfo>(query,
					new { login, accountId = userInfo.AccountId, productId = productId });
			}
		}

		public bool CreateUser(LauncherUserInfo user, string password) {
			RequireAdminFor("управления пользователями");

			using(var connection = new MySqlConnection(connectionString)) {
				try {
					connection.Open();

					int taken = connection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM `{UsersTable}` WHERE login = @login;",
						new { user.Login});
					if(taken != 0)
						throw new ArgumentException(LoginTaken, nameof(user));

					var tableColumns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, UsersTable);
					var (columns, parameters) = LauncherColumnMapper.MapForWrite(tableColumns, user, StructuralUserColumns);

					columns.Insert(0, "account_id");
					parameters.Add("account_id", userInfo.AccountId);

					columns.Insert(0, "product_id");
					parameters.Add("product_id", productId);

					columns.Insert(0, "password");
					parameters.Add("password", Cryptography.ComputeHash(password)); //?

					columns.Insert(0, "login");
					parameters.Add("login", user.Login);

					string query = $"INSERT INTO `{UsersTable}` ({string.Join(", ", columns.Select(c => $"`{c}`"))}) " +
						$"VALUES ({string.Join(", ", columns.Select(c => "@" + c))});";
					return connection.Execute(query, parameters) > 0;
				}
				catch(MySqlException ex) when(ex.Number == ER_DUP_ENTRY) { //логин заняли между проверкой и вставкой
					throw new ArgumentException(LoginTaken, nameof(user), ex);
				}
			}
		}

		public bool UpdateUser(LauncherUserInfo user, string password) {
			RequireAdminFor("управления пользователями");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var tableColumns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, UsersTable);
				var (columns, parameters) = LauncherColumnMapper.MapForWrite(tableColumns, user, StructuralUserColumns);
				var setParts = columns.ConvertAll(c => $"`{c}` = @{c}");

				if(!string.IsNullOrEmpty(password)) {
					setParts.Add("`password` = @password");
					parameters.Add("password", Cryptography.ComputeHash(password)); //?
				}
				if(!setParts.Any())
					return true;

				parameters.Add("id", user.Id);
				string query = $"UPDATE `{UsersTable}` SET {string.Join(", ", setParts)} WHERE id = @id;";
				connection.Execute(query, parameters);
			}
			return true;
		}

		public bool SyncWithChangeOwnPassword(string login, string newPassword) {
			if(string.IsNullOrEmpty(newPassword))
				return false;

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				int rowsAffected = connection.Execute(
					$"UPDATE `{UsersTable}` SET `password` = @password WHERE login = @login AND account_id = @acc;",
					new { login, password = Cryptography.ComputeHash(newPassword), acc = userInfo.AccountId });

				return rowsAffected > 0;
			}
		}

		public bool DeleteUser(LauncherUserInfo user) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				using(var transaction = connection.BeginTransaction()) {
					bool deleted = DeleteUser(user, transaction);
					transaction.Commit();
					return deleted;
				}
			}
		}

		/// <summary>Транзакцию коммитит вызывающий</summary>
		public bool DeleteUser(LauncherUserInfo user, MySqlTransaction transaction) {
			RequireAdminFor("управления пользователями");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			transaction.Connection.Execute("DELETE FROM `base_access` WHERE `user_id` = @userId", new { userId = user.Id }, transaction);
			int rowsAffected = transaction.Connection.Execute($"DELETE FROM `{UsersTable}` WHERE `id` = @userId", new { userId = user.Id }, transaction);

			return rowsAffected > 0;
		}

		/// <summary>Возвращает число сопоставленных учёток</summary>
		public int SyncUsers(IEnumerable<string> realLogins) {
			RequireAdminFor("синхронизации пользователей");

			var present = (realLogins
				?? Enumerable.Empty<string>())
					.Where(l => !string.IsNullOrEmpty(l))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var columns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, UsersTable);
				if(!columns.Contains("disabled", StringComparer.OrdinalIgnoreCase))
					return 0;

				if(!present.Any()) {
					connection.Execute(
						$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE account_id = @acc;",
						new { acc = userInfo.AccountId });
					return 0;
				}

				connection.Execute(
					$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE account_id = @acc AND login NOT IN @present;",
					new { acc = userInfo.AccountId, present });

				return present.Count;
			}
		}

		public IEnumerable<BaseAccessRow> GetUserBaseAccess(LauncherUserInfo user) {
			RequireAdminFor("просмотра доступов пользователей");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				string query = @"SELECT b.id AS BaseId, COALESCE(b.real_name, b.base_name, '') AS BaseName,
						COALESCE(b.base_title, b.base_name) AS BaseTitle,
						(ba.user_id IS NOT NULL) AS HasAccess,
						COALESCE(ba.admin, 0) AS Admin,
						COALESCE(ba.read_only, 0) AS ReadOnly
					FROM bases b
					LEFT JOIN base_access ba ON ba.base_id = b.id AND ba.user_id = @userId
					WHERE b.account_id = @accountId AND b.product_id = @productId
					ORDER BY BaseTitle;";
				return connection.Query<BaseAccessRow>(query,
					new { userId = user.Id, accountId = userInfo.AccountId, productId }).ToList();
			}
		}

		public bool ChangeBaseAccess(BaseAccessRow access, LauncherUserInfo user) {
			RequireAdminFor("изменения доступов");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var baseId = connection.QueryFirstOrDefault<int?>(
					"SELECT id FROM bases WHERE real_name = @name AND account_id = @account AND product_id = @productId;",
					new { name = access.BaseName, account = userInfo.AccountId, productId });
				if(baseId == null)
					throw new ArgumentException("База не найдена", nameof(access));

				if(!access.HasAccess) {
					connection.Execute("DELETE FROM base_access WHERE user_id = @uid AND base_id = @bid;",
						new { uid = user.Id, bid = baseId });
				}
				else {
					bool readOnly = !access.Admin && access.ReadOnly;
					var affected = connection.Execute(
						"INSERT INTO base_access (user_id, base_id, admin, read_only) " +
						"VALUES (@uid, @bid, @admin, @readOnly) " +
						"ON DUPLICATE KEY UPDATE admin = VALUES(admin), read_only = VALUES(read_only);",
						new { uid = user.Id, bid = baseId, access.Admin, readOnly });
					if(affected == 0)
						return false;
				}
			}
			return true;
		}

		public void GrantCreatorAccess(MySqlConnection connection, MySqlTransaction transaction, int baseId) {
			connection.Execute(
				"INSERT INTO base_access (user_id, base_id, admin) VALUES (@user_id, @base_id, 1) " +
				"ON DUPLICATE KEY UPDATE admin = VALUES(admin);",
				new { user_id = userInfo.Id, base_id = baseId }, transaction);
		}

		private void RequireAdminFor(string action) {
			if(!IsAdmin())
				throw new UnauthorizedAccessException($"Недостаточно прав для {action}");
		}

		private bool IsAdmin() {
			if(isAdminCached.HasValue)
				return isAdminCached.Value;

			bool result = userInfo.IsAccountAdmin || HasAnyBaseAdminAccess();
			isAdminCached = result;
			return result;
		}

		private bool HasAnyBaseAdminAccess() {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.ExecuteScalar<int>(
					"SELECT EXISTS(SELECT 1 FROM base_access WHERE user_id = @id AND admin = 1);",
					new { id = userInfo.Id }) == 1;
			}
		}

		private LauncherUserInfo GetLauncherUserInfo(string login) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var query = @"SELECT `server_users`.`id` AS Id, `server_users`.`login` AS Login, `server_users`.`password` AS PasswordHash,
       				`accounts`.`id` AS AccountId, accounts.login AS AccountName, server_users.is_account_admin AS IsAccountAdmin
					FROM `server_users` JOIN accounts ON accounts.id = `server_users`.`account_id`
					WHERE `server_users`.`login` = @login;";
				return connection.QueryFirstOrDefault<LauncherUserInfo>(query, new { login });
			}
		}
	}
}
