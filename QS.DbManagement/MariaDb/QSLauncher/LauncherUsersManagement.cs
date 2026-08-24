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
		private readonly byte productId;
		private readonly LauncherBasesManagement bases;
		private readonly LauncherUserInfo userInfo;

		public LauncherUsersManagement(MySqlConnectionStringBuilder connectionBuilder, string login, byte productId,
			LauncherBasesManagement bases) {
			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;
			this.productId = productId;
			this.bases = bases ?? throw new ArgumentNullException(nameof(bases));

			userInfo = GetLauncherUserInfo(login)
				?? throw new ArgumentException("Не удалось получить информацию о текущем пользователе", nameof(login));
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

				// product_id обязателен: список логинов собран для нашего продукта, и без него
				// синхронизация погасила бы пользователей соседнего продукта на том же сервере
				if(!present.Any()) {
					connection.Execute(
						$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE product_id = @pid;",
						new { pid = productId });
					return 0;
				}

				connection.Execute(
					$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE product_id = @pid AND login NOT IN @present;",
					new { pid = productId, present });

				return present.Count;
			}
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
				new { user_id = userInfo.Id, base_id = baseId }, transaction);
		}

		private int RequireBaseId(MySqlConnection connection, string baseName) {
			int baseId = bases.FindBaseId(connection, baseName);
			if(baseId <= 0)
				throw new ArgumentException(BaseNotFound, nameof(baseName));
			return baseId;
		}

		#endregion

		private void RequireAdminFor(string action) {
			if(!userInfo.IsAdmin)
				throw new UnauthorizedAccessException($"Недостаточно прав для {action}");
		}

		private LauncherUserInfo GetLauncherUserInfo(string login) {
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.QueryFirstOrDefault<LauncherUserInfo>(
					$"SELECT {UserSelect} FROM `{UsersTable}` WHERE `login` = @login AND `product_id` = @productId;",
					new { login, productId });
			}
		}
	}
}
