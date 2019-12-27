using System;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.DB;
using QS.Project.Domain;
using System.Collections.Generic;
using System.Linq;
using QS.Services;

namespace QS.Project.Repositories
{
	public class MySQLUserRepository
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IMySQLProvider mysqlProvider;
		private readonly IInteractiveService interactiveService;

		public MySQLUserRepository(IMySQLProvider mysqlProvider, IInteractiveService interactiveService)
		{
			this.mysqlProvider = mysqlProvider ?? throw new ArgumentNullException(nameof(mysqlProvider));
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public string GetPasswordProxy()
		{
			return "n0tChanG3d";
		}

		public UserBase GetUser(int id)
		{
			UserBase user = new UserBase();

			logger.Info("Запрос пользователя №{0}...", id);
			string sql = "SELECT * FROM users WHERE users.id = @id";

			MySqlDataReader rdr = null;
			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", id);

				rdr = cmd.ExecuteReader();

				var valueReader = new DBValueReader(rdr);

				rdr.Read();

				user.Id = valueReader.GetInt("id", 0);
				user.Name = valueReader.GetString("name");
				user.Login = valueReader.GetString("login");
				user.Deactivated = valueReader.GetBoolean("deactivated", false);
				user.Email = valueReader.GetString("email");
				user.IsAdmin = valueReader.GetBoolean("admin", false);
				user.Description = valueReader.GetString("description");

				logger.Info("Ok");
				return user;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка получения информации о пользователе!");
				throw ex;
			} finally {
				if(rdr != null) {
					rdr.Close();
				}
			}
		}

		public Dictionary<string, string> GetExtraFieldValues(int userId, IEnumerable<string> extraFieldNames)
		{
			var result = new Dictionary<string, string>();

			logger.Info("Запрос дополнительных полей пользователя №{0}...", userId);
			string sql = "SELECT * FROM users WHERE users.id = @id";

			MySqlDataReader rdr = null;
			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", userId);

				rdr = cmd.ExecuteReader();

				var valueReader = new DBValueReader(rdr);

				rdr.Read();

				foreach(var fieldName in extraFieldNames) {
					result.Add(fieldName, valueReader.GetString(fieldName, null));
				}

				logger.Info("Ok");
				return result;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка получения информации о пользователе!");
				throw ex;
			} finally {
				if(rdr != null) {
					rdr.Close();
				}
			}
		}

		public int GetUserId(string login)
		{
			UserBase user = new UserBase();

			logger.Info("Запрос пользователя логин {0}...", login);
			string sql = "SELECT id FROM users WHERE users.login = @login";

			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@login", login);

				int result = (int)cmd.ExecuteScalar();
				logger.Info("Ok");
				return result;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка получения информации о пользователе!");
				return 0;
			}
		}

		private IEnumerable<string> LoadUserPermissions(int userId)
		{
			logger.Info("Загрузка прав пользователя №{0}...", userId);

			if(userId == 0) {
				logger.Info("Попытка загрузить права неизвестного пользователя");
				return new string[0];
			}
			string sql = "SELECT * FROM permission_preset_user WHERE user_id = @id";

			MySqlDataReader rdr = null;
			try {
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", userId);

				rdr = cmd.ExecuteReader();
				var valueReader = new DBValueReader(rdr);

				var result = new List<string>();
				while(rdr.Read()) {
					result.Add(valueReader.GetString("permission_name"));
				}
				return result;
			} catch(Exception ex) {
				return new string[0];
			} finally {
				if(rdr != null) {
					rdr.Close();
				}
			}
		}

		private bool CreateLogin(string login, string password)
		{
			logger.Info("Создание учетной записи на сервере...");

			try {
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) FROM users WHERE login = @login";
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
					string message = "Пользователь с логином " + login + " уже существует в базе. " +
									 "Создание второго пользователя с таким же логином невозможно.";
					interactiveService.ShowMessage(Dialog.ImportanceLevel.Error, message, "Создание учетной записи на сервере");
					return false;
				}

				sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				try {
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
						string message = "Пользователь с логином " + login + " уже существует на сервере. " +
										 "Если он использовался для доступа к другим базам, может возникнуть путаница. " +
										 "Использовать этот логин?";
						bool result = interactiveService.Question(message, "Создание учетной записи на сервере");
						return result;
					}
				} catch(MySqlException ex) {
					if(ex.Number == 1045) {
						logger.Warn(ex, "Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					} else {
						return false;
					}
				}
				//Создание пользователя.
				sql = "CREATE USER @login IDENTIFIED BY @password";
				cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				cmd.Parameters.AddWithValue("@password", password);
				cmd.ExecuteNonQuery();
				cmd.CommandText = "CREATE USER @login @'localhost' IDENTIFIED BY @password";
				cmd.ExecuteNonQuery();

				logger.Info("Ok");
				return true;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка создания пользователя!");
				throw ex;
			}
		}

		public bool RenameLogin(string oldLogin, string newLogin)
		{
			logger.Info("Переименование учетной записи на сервере...");
			try {
				mysqlProvider.CheckConnectionAlive();
				//Проверка существует ли логин
				string sql = "SELECT COUNT(*) from mysql.user WHERE USER = @login";
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", newLogin);
				try {
					if(Convert.ToInt32(cmd.ExecuteScalar()) > 0) {
						string message = "Пользователь с логином " + newLogin + " уже существует на сервере. " +
										 "Переименование невозможно.";
						interactiveService.ShowMessage(Dialog.ImportanceLevel.Error, message, "Переименование учетной записи на сервере");
						return false;
					}
				} catch(MySqlException ex) {
					if(ex.Number == 1045) {
						logger.Warn(ex, "Нет доступа к таблице пользователей, пробую создать пользователя в слепую.");
					} else {
						return false;
					}
				}

				//Переименование пользователя.
				sql = "RENAME USER @old_login TO @new_login, @old_login @'localhost' TO @new_login @'localhost'";
				cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@old_login", oldLogin);
				cmd.Parameters.AddWithValue("@new_login", newLogin);
				cmd.ExecuteNonQuery();
				logger.Info("Ok");
				return true;
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка переименования пользователя!");
				throw ex;
			}
		}

		public void ChangePassword(string login, string password)
		{
			logger.Info("Отправляем новый пароль на сервер...");

			try {
				mysqlProvider.CheckConnectionAlive();
				string sql = "SET PASSWORD FOR @login = PASSWORD(@password)";
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				cmd.Parameters.AddWithValue("@password", password);
				cmd.ExecuteNonQuery();
				cmd.CommandText = "SET PASSWORD FOR @login @'localhost' = PASSWORD(@password)";
				cmd.ExecuteNonQuery();
				logger.Info("Пароль изменен. Ok");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка установки пароля!");
				throw ex;
			}
			
		}

		public void UpdatePrivileges(string login, bool isAdmin)
		{
			logger.Info("Устанавливаем права...");
			try {
				string privileges;
				if(isAdmin)
					privileges = "ALL";
				else
					privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, SHOW VIEW";
				string sql = $"GRANT {privileges} ON `{mysqlProvider.DbConnection.Database}`.* TO @login, @login @'localhost'";
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				cmd.ExecuteNonQuery();
				if(isAdmin) {
					cmd.CommandText = "GRANT CREATE USER, GRANT OPTION ON *.* TO @login, @login @'localhost'";
				} else {
					cmd.CommandText = "REVOKE CREATE USER, GRANT OPTION ON *.* FROM @login, @login @'localhost'";
				}
				cmd.ExecuteNonQuery();
				bool GrantMake = false;
				if(isAdmin) {
					sql = "GRANT SELECT, UPDATE ON mysql.* TO @login, @login @'localhost'";
					GrantMake = true;
				} else {
					sql = "SHOW GRANTS FOR @login";
					cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
					cmd.Parameters.AddWithValue("@login", login);
					using(MySqlDataReader rdr = cmd.ExecuteReader()) {
						while(rdr.Read()) {
							if(rdr.GetString(0).IndexOf("mysql") > 0) {
								GrantMake = true;
							}
						}
					}
					sql = "REVOKE SELECT, UPDATE ON mysql.* FROM @login, @login @'localhost'";
				}
				if(GrantMake) {
					cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
					cmd.Parameters.AddWithValue("@login", login);
					cmd.ExecuteNonQuery();
				}
				logger.Info("Права установлены. Ok");
			} catch(MySqlException ex) when(ex.Number == 1044) {
				logger.Error(ex, "Ошибка установки прав!");
				string message = "У вас не достаточно прав на сервере MySQL для установки полномочий другим пользователям. Возможно некоторые права не были установлены.";
				interactiveService.ShowMessage(Dialog.ImportanceLevel.Error, message, "Установка прав");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка установки прав!");
				throw ex;
			}
		}
	
		public void CreateUser(UserBase user, string password, string extraFieldsForSelect, string extraFieldsForInsert, Dictionary<string, string> permissionsValues)
		{
			if(user.Id > 0 || !CreateLogin(user.Login, password)) {
				return;
			}
			string sql = "INSERT INTO users (name, login, deactivated, email, admin, description" + extraFieldsForSelect + ") " +
				  "VALUES (@name, @login, @deactivated, @email, @admin, @description" + extraFieldsForInsert + ")";

			logger.Info("добавление пользователя...");

			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@name", user.Name);
				cmd.Parameters.AddWithValue("@login", user.Login);
				cmd.Parameters.AddWithValue("@admin", user.IsAdmin);
				cmd.Parameters.AddWithValue("@deactivated", user.Deactivated);
				cmd.Parameters.AddWithValue("@email", user.Email);

				foreach(var item in permissionsValues) {
					cmd.Parameters.AddWithValue(item.Key, item.Value);
				}

				cmd.Parameters.AddWithValue("@description", user.Description);
				cmd.ExecuteNonQuery();
				int userId = GetUserId(user.Login);

			} catch(Exception ex) {
				logger.Error(ex, "Ошибка записи пользователя!");
				throw ex;
			}
		}

		private void UpdateUserPresetPermissions(int userId, Dictionary<string, bool> userPresetPermissions)
		{
			logger.Info("Загрузка прав пользователя №{0}...", userId);

			if(userId == 0) {
				logger.Warn("Попытка обновить права неизвестного пользователя");
				return;
			}

			var activeUserPermissions = LoadUserPermissions(userId);


			var insertingPermissions = userPresetPermissions.Where(x => x.Value).Where(x => !activeUserPermissions.Contains(x.Key)).Select(x => x.Key);
			string insertingPermissionsParam = "";
			foreach(var item in insertingPermissions) {
				insertingPermissionsParam += $"(\"{item}\", {userId}),"; 
			}
			insertingPermissionsParam = insertingPermissionsParam.Trim(',');

			var deletingPermissions = userPresetPermissions.Where(x => !x.Value).Where(x => activeUserPermissions.Contains(x.Key)).Select(x => x.Key);
			var deletingPermissionsParam = string.Join("\", \"", deletingPermissions).Trim(',', ' ', '"');

			string insertSql = "INSERT INTO permission_preset_user (permission_name, user_id) VALUES " + insertingPermissionsParam + ";";
			string deleteSql = "DELETE FROM permission_preset_user WHERE user_id = @id AND permission_name IN (" + deletingPermissionsParam + ");";

			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(deleteSql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", userId);

				var result = cmd.ExecuteNonQuery();
				logger.Info($"Удалено {result} прав для пользователя {userId}");
			} catch(Exception ex) {
				return;
			}

			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(insertSql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", userId);

				var result = cmd.ExecuteNonQuery();
				logger.Info($"Добавлено {result} прав для пользователя {userId}");
			} catch(Exception ex) {
				return;
			}
		}

		public void UpdateUser(UserBase user, string password, string extraFieldsForUpdate, Dictionary<string, string> permissionsValues)
		{
			var originUser = GetUser(user.Id);

			if(originUser.Login != user.Login && !RenameLogin(originUser.Login, user.Login)) {
				return;
			}

			if(password != GetPasswordProxy()) {
				ChangePassword(user.Login, password);
			}

			var sql = "UPDATE users SET name = @name, deactivated = @deactivated, email = @email, login = @login, admin = @admin," +
				  "description = @description " + extraFieldsForUpdate + " WHERE id = @id";

			UpdatePrivileges(user.Login, user.IsAdmin);
			logger.Info("Запись пользователя...");

			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);

				cmd.Parameters.AddWithValue("@id", user.Id);
				cmd.Parameters.AddWithValue("@name", user.Name);
				cmd.Parameters.AddWithValue("@login", user.Login);
				cmd.Parameters.AddWithValue("@admin", user.IsAdmin);
				cmd.Parameters.AddWithValue("@deactivated", user.Deactivated);
				cmd.Parameters.AddWithValue("@email", user.Email);
				cmd.Parameters.AddWithValue("@description", user.Description);

				foreach(var item in permissionsValues) {
					cmd.Parameters.AddWithValue(item.Key, item.Value);
				}

				cmd.ExecuteNonQuery();
				logger.Info("Ok");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка записи пользователя!");
				throw ex;
			}
		}

		public void DropUser(string login)
		{
			logger.Info("Удаляем пользователя MySQL...");
			string sql;
			sql = "DROP USER @login, @login @'localhost'";
			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.Parameters.AddWithValue("@login", login);
				cmd.ExecuteNonQuery();
				logger.Info("Пользователь удалён. Ok");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка удаления пользователя!");
				throw ex;
			}
		}
	}
}
