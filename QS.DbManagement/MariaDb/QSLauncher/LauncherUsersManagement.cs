using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherUsersManagement
	{
		private const string LauncherBaseName = LauncherMetadataManagement.LauncherBaseName;
		private const string UsersTable = "server_users";
		private const string UpdateRightsTable = "base_update_rights";
		private const int ER_DUP_ENTRY = 1062;

		private const string UserNotFound = "Пользователь с указанным именем не найден";
		private const string LoginTaken = "Такое имя пользователя уже занято";
		private const string BaseNotFound = "База не найдена";

		private const string UserSelect =
			"`id` AS Id, `login` AS Login, `name` AS Name, " +
			"`email` AS Email, `phone` AS Phone, `is_admin` AS IsAdmin, `disabled` AS Disabled";

		private static readonly string[] UserWritableColumns = { "name", "email", "is_admin", "disabled" };

		private readonly string connectionString;
		private readonly string login;
		private readonly bool isAdmin;
		private readonly byte productId;
		private readonly LauncherBasesManagement bases;

		public LauncherUsersManagement(MySqlConnectionStringBuilder connectionBuilder, string login, bool isAdmin,
			byte productId, LauncherBasesManagement bases) {
			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;
			this.login = login ?? throw new ArgumentNullException(nameof(login));
			this.isAdmin = isAdmin;
			this.productId = productId;
			this.bases = bases ?? throw new ArgumentNullException(nameof(bases));
		}

		public IEnumerable<LauncherUserInfo> GetUsers() {
			RequireAdminFor("просмотра пользователей");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.Query<LauncherUserInfo>(
					$"SELECT {UserSelect} FROM `{UsersTable}` WHERE product_id = @productId ORDER BY login;",
					new { productId }).ToList();
			}
		}

		public LauncherUserInfo GetUserByLogin(string login) {
			RequireAdminFor("просмотра пользователя");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.QueryFirstOrDefault<LauncherUserInfo>(
					$"SELECT {UserSelect} FROM `{UsersTable}` WHERE login = @login AND product_id = @productId;",
					new { login, productId });
			}
		}

		public bool CreateUser(LauncherUserInfo user) {
			RequireAdminFor("управления пользователями");

			using(var connection = new MySqlConnection(connectionString)) {
				try {
					connection.Open();

					int taken = connection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM `{UsersTable}` WHERE login = @login;",
						new { user.Login });
					if(taken != 0)
						throw new ArgumentException(LoginTaken, nameof(user));

					var columns = new List<string> { "login", "product_id" };
					columns.AddRange(UserWritableColumns);

					string query = $"INSERT INTO `{UsersTable}` ({string.Join(", ", columns.Select(c => $"`{c}`"))}) " +
						$"VALUES ({string.Join(", ", columns.Select(c => "@" + c))});";
					return connection.Execute(query, InsertParameters(user)) > 0;
				}
				catch(MySqlException ex) when(ex.Number == ER_DUP_ENTRY) { //логин заняли между проверкой и вставкой
					throw new ArgumentException(LoginTaken, nameof(user), ex);
				}
			}
		}

		private DynamicParameters InsertParameters(LauncherUserInfo user) {
			var parameters = MakeWritableParameters(user);
			parameters.Add("login", user.Login);
			parameters.Add("product_id", productId);
			return parameters;
		}
		private static DynamicParameters MakeWritableParameters(LauncherUserInfo user) {
			var parameters = new DynamicParameters();
			parameters.Add("name", Blank(user.Name));
			parameters.Add("email", Blank(user.Email));
			parameters.Add("is_admin", user.IsAdmin);
			parameters.Add("disabled", user.Disabled);
			return parameters;
		}
		/// <summary>Пустую строку из формы кладём как NULL</summary>
		private static string Blank(string value) => string.IsNullOrEmpty(value) ? null : value;

		public bool UpdateUser(LauncherUserInfo user) {
			RequireAdminFor("управления пользователями");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var parameters = MakeWritableParameters(user);
				parameters.Add("id", user.Id);

				string assignments = string.Join(", ", UserWritableColumns.Select(c => $"`{c}` = @{c}"));
				connection.Execute($"UPDATE `{UsersTable}` SET {assignments} WHERE id = @id;", parameters);
			}
			return true;
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

			transaction.Connection.Execute($"DELETE FROM `{UpdateRightsTable}` WHERE `user_id` = @userId", new { userId = user.Id }, transaction);
			int rowsAffected = transaction.Connection.Execute($"DELETE FROM `{UsersTable}` WHERE `id` = @userId", new { userId = user.Id }, transaction);

			return rowsAffected > 0;
		}

		/// <returns>число учёток сервера, сопоставленных с метабазой</returns>
		public int SyncUsers(IEnumerable<LauncherUserInfo> serverUsers) {
			RequireAdminFor("синхронизации пользователей");

			var present = (serverUsers ?? Enumerable.Empty<LauncherUserInfo>())
				.Where(u => !string.IsNullOrEmpty(u?.Login))
				.ToList();

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				DisableMissingUsers(connection, present.ConvertAll(u => u.Login));
				InsertMissingUsers(connection, present);
				return present.Count;
			}
		}
		private void DisableMissingUsers(MySqlConnection connection, List<string> presentLogins) {
			var keep = presentLogins.Concat(MySqlSystemObjects.Users).ToList();

			connection.Execute(
				$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE product_id = @pid AND login NOT IN @keep;",
				new { pid = productId, keep });
		}

		private void InsertMissingUsers(MySqlConnection connection, IEnumerable<LauncherUserInfo> serverUsers) {
			connection.QueryMultiple(
				$"INSERT INTO `{UsersTable}` (`login`, `product_id`, `is_admin`, `disabled`) " +
				"VALUES (@login, @product_id, @is_admin, @disabled) " +
				"ON DUPLICATE KEY UPDATE `login` = `login`;",
				serverUsers.Select(user => new {
					login = user.Login,
					product_id = productId,
					is_admin = user.IsAdmin,
					disabled = user.Disabled
				}).ToList());
		}

		#region Право на обновление базы

		public bool SetBaseUpdateRight(string baseName, LauncherUserInfo user, bool canUpdate) {
			RequireAdminFor("изменения прав на обновление баз");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				int baseId = RequireBaseId(connection, baseName);

				connection.Execute(
					$"INSERT INTO `{UpdateRightsTable}` (user_id, base_id, can_update) VALUES (@uid, @bid, @canUpdate) " +
					"ON DUPLICATE KEY UPDATE can_update = VALUES(can_update);",
					new { uid = user.Id, bid = baseId, canUpdate });
			}
			return true;
		}

		/// <summary>
		/// Снимает запись о праве обновлять базу. На доступ к базе не влияет:
		/// видимость и доступ живут в GRANT'ах сервера
		/// </summary>
		public bool RevokeBaseUpdateRight(string baseName, LauncherUserInfo user) {
			RequireAdminFor("изменения прав на обновление баз");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				int baseId = RequireBaseId(connection, baseName);

				connection.Execute($"DELETE FROM `{UpdateRightsTable}` WHERE user_id = @uid AND base_id = @bid;",
					new { uid = user.Id, bid = baseId });
			}
			return true;
		}

		/// <summary>Создавший базу может её обновлять</summary>
		public void GrantCreatorUpdateRight(MySqlConnection connection, MySqlTransaction transaction, int baseId) {
			connection.Execute(
				$"INSERT INTO `{UpdateRightsTable}` (user_id, base_id, can_update) VALUES (@user_id, @base_id, 1) " +
				"ON DUPLICATE KEY UPDATE can_update = VALUES(can_update);",
				new { user_id = EnsureOwnRow(connection, transaction), base_id = baseId }, transaction);
		}

		private int EnsureOwnRow(MySqlConnection connection, MySqlTransaction transaction) {
			connection.Execute(
				$"INSERT INTO `{UsersTable}` (`login`, `product_id`, `is_admin`) VALUES (@login, @product_id, @is_admin) " +
				"ON DUPLICATE KEY UPDATE `login` = `login`;",
				new { login, product_id = productId, is_admin = isAdmin }, transaction);

			// логин уникален на всю метабазу, поэтому product_id в условии не нужен
			return connection.ExecuteScalar<int>(
				$"SELECT `id` FROM `{UsersTable}` WHERE `login` = @login;", new { login }, transaction);
		}

		private int RequireBaseId(MySqlConnection connection, string baseName) {
			int baseId = bases.FindBaseId(connection, baseName);
			if(baseId <= 0)
				throw new ArgumentException(BaseNotFound, nameof(baseName));
			return baseId;
		}

		#endregion

		private void RequireAdminFor(string action) {
			// право на весь сервер; своей записи в метабазе при этом может и не быть
			if(!isAdmin)
				throw new UnauthorizedAccessException($"Недостаточно прав для {action}");
		}
	}
}
