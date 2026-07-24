using Dapper;
using MySqlConnector;
using QS.Cloud;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherUsersManagement {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string LauncherBaseName = "QSLauncher";

		private bool CanWrite;
		private string ConnectionString;
		private int ProductId;
		private LauncherUserInfo UserInfo;

		public LauncherUsersManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, string login, int productId) {
			connectionBuilder.Database = LauncherBaseName;
			connectionBuilder.AllowLoadLocalInfile = true;
			ConnectionString = connectionBuilder.ConnectionString;

			CanWrite = canWrite;
			UserInfo = GeLauncherUserInfo(login);
			if(UserInfo == null)
				throw new ArgumentException("Не удалось получить информацию о текущем пользователе");

			ProductId = productId;
		}

		public IEnumerable<LauncherUserInfo> GetUsers() {
			IEnumerable<LauncherUserInfo> response;
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для просмотра пользователей");

			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				string query = "SELECT id, login, name, email, phone, post, comment, disabled, is_account_admin AS IsAccountAdmin " +
					"FROM server_users WHERE account_id = @accountId ORDER BY login;"; //?
				response = connection.Query<LauncherUserInfo>(query, new { accountId = UserInfo.AccountId });
			}
			return response;
		}

		//Регистрация пользователя в облаке
		public bool CreateUser(LauncherUserInfo user, string password) {
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для управления пользователями");
			using(var connection = new MySqlConnection(ConnectionString)) {
				try {
					connection.Open();

					//Проверка того что имя пользователя свободно
					string query = "SELECT COUNT(*) FROM server_users WHERE login = @login AND account_id = @id;";
					int usernameFree = connection.ExecuteScalar<int>(query, new { user.Login, id = UserInfo.AccountId });
					if(usernameFree != 0)
						throw new ArgumentException("Такое имя пользователя уже занято");

					query = "INSERT INTO server_users (login, password, account_id, name, email, phone, post, comment, is_account_admin) " +
						"VALUES (@login, @pass, @account_id, @name, @email, @phone, @post, @comment, @admin);";
					var res = connection.Execute(query, new {
						login = user.Login,
						pass = Cryptography.ComputeHash(password),
						account_id = user.AccountId,
						name = NullIfEmpty(user.Name),
						email = NullIfEmpty(user.Email),
						phone = NullIfEmpty(user.Phone),
						post = NullIfEmpty(user.Post),
						comment = NullIfEmpty(user.Comment),
						admin = user.IsAccountAdmin
					});
					if(res == 0)
						return false;
					return true;
				}
				catch(MySqlException ex) when(ex.Number == 1062) //ER_DUP_ENTRY
				{
					throw new ArgumentException("Такое имя пользователя уже занято");
				}
			}
		}

		public bool UpdateUser(LauncherUserInfo user, string password) {
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для управления пользователями");
			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				if(user.Id > 0)
					throw new ArgumentException("Пользователь с указанным именем не найден");

				string query = "UPDATE server_users SET name = @name, email = @email, phone = @phone, " +
					"post = @post, comment = @comment, disabled = @disabled, is_account_admin = @admin";
				var args = new DynamicParameters(new {
					name = NullIfEmpty(user.Name),
					email = NullIfEmpty(user.Email),
					phone = NullIfEmpty(user.Phone),
					post = NullIfEmpty(user.Post),
					comment = NullIfEmpty(user.Comment),
					disabled = user.Disabled,
					admin = user.IsAccountAdmin,
					id = user.Id
				});
				// Пароль хешируем и передаём только если он действительно меняется
				if(!string.IsNullOrEmpty(password)) {
					query += ", password = @password";
					args.Add("password", Cryptography.ComputeHash(password));
				}
				query += " WHERE id = @id;";

				connection.Execute(query, args);
			}
			return true;
		}

		public bool DeleteUser(LauncherUserInfo user) {
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для управления пользователями");
			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				if(user.Id > 0)
					throw new ArgumentException("Пользователь с указанным именем не найден");

				// Доступы и сам пользователь удаляются атомарно
				int rowsAffected;
				using(var transaction = connection.BeginTransaction()) {
					string query = "DELETE FROM `base_access` WHERE `user_id` = @userId";
					connection.Execute(query, new { userId = user.Id }, transaction);
					query = "DELETE FROM `server_users` WHERE `id` = @userId";
					rowsAffected = connection.Execute(query, new { userId = user.Id }, transaction);
					transaction.Commit();
				}
				if(rowsAffected > 0)
					return true;
				else
					return false;
			}
		}

		public IEnumerable<BaseAccessRow> GetUserBaseAccess(LauncherUserInfo user) {
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для просмотра доступов пользователей");

			IEnumerable<BaseAccessRow> response;
			using(var connection = new MySqlConnection(ConnectionString)) {
				if(user.Id > 0)
					throw new ArgumentException("Пользователь с указанным именем не найден");

				string query = @"SELECT b.id AS BaseId, COALESCE(b.base_title, b.base_name) AS BaseTitle,
						(ba.user_id IS NOT NULL) AS HasAccess,
						COALESCE(ba.admin, 0) AS Admin,
						COALESCE(ba.read_only, 0) AS ReadOnly
					FROM bases b
					LEFT JOIN base_access ba ON ba.base_id = b.id AND ba.user_id = @userId
					WHERE b.account_id = @accountId AND b.product_id = @productId
					ORDER BY BaseTitle;";
				response = connection.Query<BaseAccessRow>(query, new { userId = user.Id, accountId = user.AccountId, productId = ProductId });
			}
			return response;
		}

		public bool ChangeBaseAccess(BaseAccessRow access, LauncherUserInfo user) {
			if(UserInfo == null || !UserInfo.IsAccountAdmin)
				throw new AccessViolationException("Недостаточно прав для изменения доступов");

			using(var connection = new MySqlConnection(ConnectionString)) {
				if(user.Id > 0)
					throw new ArgumentException("Пользователь с указанным именем не найден");

				var baseId = connection.QueryFirstOrDefault<int?>(
					"SELECT id FROM bases WHERE id = @bid AND account_id = @account AND product_id = @productId;",
					new { bid = access.BaseId, account = UserInfo.AccountId, productId = ProductId });
				if(baseId == null)
					throw new ArgumentException("База не найдена");

				if(!access.HasAccess) {
					string query = "DELETE FROM base_access WHERE user_id = @uid AND base_id = @bid;";
					connection.Execute(query, new { uid = user.Id, bid = access.BaseId });
				}
				else {
					bool readOnly = !access.Admin && access.ReadOnly;
					string query = "INSERT INTO base_access (user_id, base_id, admin, read_only) " +
						"VALUES (@uid, @bid, @admin, @readOnly) " +
						"ON DUPLICATE KEY UPDATE admin = VALUES(admin), read_only = VALUES(read_only);";
					var affected = connection.Execute(query, new { uid = user.Id, bid = access.BaseId, access.Admin, readOnly });
					if(affected == 0)
						return false;
				}
			}
			return true;
		}

		private static string NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

		private LauncherUserInfo GeLauncherUserInfo(string login) {
			using(var connection = new MySqlConnection(ConnectionString)) {
				var query = $@"SELECT `server_users`.`id` as Id, `server_users`.`login` as Login, `server_users`.`password` as PasswordHash, 
       				`accounts`.`id` as AccountId, accounts.login as AccountName, server_users.is_account_admin as IsAccountAdmin
					FROM `server_users` JOIN accounts ON accounts.id = `server_users`.`account_id` 
					WHERE `server_users`.`login` = @login;";
				var userInfo = connection.QueryFirstOrDefault<LauncherUserInfo>(query, new { login = login });
				if(userInfo == null)
					return null;

				return userInfo;
			}
		}
	}
}
