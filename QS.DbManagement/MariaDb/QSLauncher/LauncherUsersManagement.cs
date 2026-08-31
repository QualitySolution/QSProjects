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

		private readonly string connectionString;
		private readonly string login;
		private readonly bool isAdmin;
		private readonly LauncherBasesManagement bases;

		public LauncherUsersManagement(MySqlConnectionStringBuilder connectionBuilder, string login, bool isAdmin,
			LauncherBasesManagement bases) {
			// строку правим на копии: builder принадлежит вызывающему
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;
			this.login = login ?? throw new ArgumentNullException(nameof(login));
			this.isAdmin = isAdmin;
			this.bases = bases ?? throw new ArgumentNullException(nameof(bases));
		}

		public IEnumerable<LauncherUserInfo> GetUsers() {
			RequireAdminFor("просмотра пользователей");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.Query<LauncherUserInfo>(
					$"SELECT {UserSelect} FROM `{UsersTable}` ORDER BY login;").ToList();
			}
		}

		public LauncherUserInfo GetUserByLogin(string login) {
			RequireAdminFor("просмотра пользователя");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				return connection.QueryFirstOrDefault<LauncherUserInfo>(
					$"SELECT {UserSelect} FROM `{UsersTable}` WHERE login = @login;",
					new { login });
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

					DynamicParameters parameters = new DynamicParameters();
					FillCreateUserCollumnsParams(user, parameters);

					string query = $"INSERT INTO `{UsersTable}` ({GetLabelsUserStr(UserCreateColumns)}) " +
						$"VALUES ({GetValuesUserStr(UserCreateColumns)});";
					return connection.Execute(query, parameters) > 0;
				}
				catch(MySqlException ex) when(ex.Number == ER_DUP_ENTRY) { //логин заняли между проверкой и вставкой
					throw new ArgumentException(LoginTaken, nameof(user), ex);
				}
			}
		}

		/// <summary>Пустую строку из формы кладём как NULL</summary>
		private static string Blank(string value) => string.IsNullOrEmpty(value) ? null : value;

		public bool UpdateUser(LauncherUserInfo user) {
			RequireAdminFor("управления пользователями");
			if(user.Id <= 0)
				throw new ArgumentException(UserNotFound, nameof(user));

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				DynamicParameters parameters = new DynamicParameters();
				FillUpdateUserCollumnsParams(user, parameters);
				parameters.Add("id", user.Id);

				connection.Execute($"UPDATE `{UsersTable}` SET {GetSetUserStr(UserUpdateColumns)} WHERE id = @id;", parameters);
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
				$"UPDATE `{UsersTable}` SET disabled = TRUE WHERE login NOT IN @keep;",
				new { keep });
		}

		private void InsertMissingUsers(MySqlConnection connection, List<LauncherUserInfo> serverUsers)
		{
			int userCount = serverUsers.Count;
			if(userCount <= 0)
				return;

			List<string> values = new List<string>(userCount);
			DynamicParameters parameters = new DynamicParameters();
			for(int i = 0; i < userCount; i++)
			{
				values.Add($"({GetValuesUserStr(UserCreateColumns, i)})");
				FillCreateUserCollumnsParams(serverUsers[i], parameters, i);
			}

			connection.Execute(
				$"INSERT INTO `{UsersTable}` ({GetLabelsUserStr(UserCreateColumns)})"
				+ " VALUES "
				+ string.Join(", ", values)
				+ " ON DUPLICATE KEY UPDATE `login` = `login`;"
				, parameters);
		}


		private static readonly string[] UserKeyColumns = { "login" };
		private static readonly string[] UserUpdateColumns = { "name", "email", "phone", "is_admin", "disabled" };
		private static readonly string[] UserCreateColumns = UserKeyColumns.Concat(UserUpdateColumns).ToArray();
		private static void FillCreateUserCollumnsParams(LauncherUserInfo user, DynamicParameters parameters, int i = 0)
		{
			parameters.Add($"login{i}", user.Login);
			FillUpdateUserCollumnsParams(user, parameters, i);
		}
		private static void FillUpdateUserCollumnsParams(LauncherUserInfo user, DynamicParameters parameters, int i = 0)
		{
			parameters.Add($"name{i}", Blank(user.Name));
			parameters.Add($"email{i}", Blank(user.Email));
			parameters.Add($"phone{i}", Blank(user.Phone));
			parameters.Add($"is_admin{i}", user.IsAdmin);
			parameters.Add($"disabled{i}", user.Disabled);
		}

		private static string GetLabelsUserStr(string[] columns)
		{
			return string.Join(", ", columns.Select(c => "`" + c + "`"));
		}
		private static string GetValuesUserStr(string[] columns, int i = 0)
		{
			return string.Join(", ", columns.Select(c => "@" + c + i.ToString()));
		}
		private static string GetSetUserStr(string[] columns, int i = 0)
		{
			return string.Join(", ", columns.Select(c => $"`{c}` = @{c}{i}"));
		}
		private const string UserSelect =
			"`id` AS Id, `login` AS Login, `name` AS Name, " +
			"`email` AS Email, `phone` AS Phone, `is_admin` AS IsAdmin, `disabled` AS Disabled";

		private int EnsureOwnRow(MySqlConnection connection, MySqlTransaction transaction)
		{
			connection.Execute(
				$"INSERT INTO `{UsersTable}` (`login`, `is_admin`) VALUES (@login, @is_admin) " +
				"ON DUPLICATE KEY UPDATE `login` = `login`;",
				new { login, is_admin = isAdmin }, transaction);

			// логин уникален на всю метабазу
			return connection.ExecuteScalar<int>(
				$"SELECT `id` FROM `{UsersTable}` WHERE `login` = @login;", new { login }, transaction);
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
