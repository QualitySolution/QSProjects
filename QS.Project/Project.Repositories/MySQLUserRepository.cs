﻿using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using NLog;
using QS.Dialog;
using QS.Project.DB;
using QS.Project.Domain;

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

		public bool CreateLogin(string login, string password)
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
				var loginEsc = MySqlHelper.EscapeString(login);
				var passwordEsc = MySqlHelper.EscapeString(password);
				sql = $"CREATE USER '{loginEsc}' IDENTIFIED BY '{passwordEsc}';";
				cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.ExecuteNonQuery();
				cmd.CommandText = $"CREATE USER '{loginEsc}'@'localhost' IDENTIFIED BY '{passwordEsc}';";
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
				var oldLoginEsc = MySqlHelper.EscapeString(oldLogin);
				var newLoginEsc = MySqlHelper.EscapeString(newLogin);
				sql = $"RENAME USER '{oldLoginEsc}' TO '{newLoginEsc}', '{oldLoginEsc}'@'localhost' TO '{newLoginEsc}'@'localhost';";
				cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
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
				var loginEsc = MySqlHelper.EscapeString(login);
				var passwordEsc = MySqlHelper.EscapeString(password);
				string sql = $"SET PASSWORD FOR '{loginEsc}' = PASSWORD('{passwordEsc}');";
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.ExecuteNonQuery();
				cmd.CommandText = $"SET PASSWORD FOR '{loginEsc}'@'localhost' = PASSWORD('{passwordEsc}');";
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
				var loginEsc = MySqlHelper.EscapeString(login);
				if(isAdmin)
					privileges = "ALL";
				else
					privileges = "SELECT, INSERT, UPDATE, DELETE, EXECUTE, SHOW VIEW";
				string sql = $"GRANT {privileges} ON `{mysqlProvider.DbConnection.Database}`.* TO '{loginEsc}', '{loginEsc}'@'localhost';";
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.ExecuteNonQuery();
				if(isAdmin) {
					cmd.CommandText = $"GRANT CREATE USER, GRANT OPTION ON *.* TO '{loginEsc}', '{loginEsc}'@'localhost'";
				} else {
					cmd.CommandText = $"REVOKE CREATE USER, GRANT OPTION ON *.* FROM '{loginEsc}', '{loginEsc}'@'localhost'";
				}
				cmd.ExecuteNonQuery();
				bool grantMake = false;
				if(isAdmin) {
					sql = $"GRANT SELECT, UPDATE ON mysql.* TO '{loginEsc}', '{loginEsc}'@'localhost'";
					grantMake = true;
				} else {
					sql = $"SHOW GRANTS FOR '{loginEsc}'";
					cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
					using(MySqlDataReader rdr = cmd.ExecuteReader()) {
						while(rdr.Read()) {
							if(rdr.GetString(0).IndexOf("mysql") > 0) {
								grantMake = true;
							}
						}
					}
					sql = $"REVOKE SELECT, UPDATE ON mysql.* FROM '{loginEsc}', '{loginEsc}'@'localhost'";
				}
				if(grantMake) {
					cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
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
			var loginEsc = MySqlHelper.EscapeString(login);
			string sql = $"DROP USER '{loginEsc}', '{loginEsc}'@'localhost';";
			try {
				mysqlProvider.CheckConnectionAlive();
				MySqlCommand cmd = new MySqlCommand(sql, mysqlProvider.DbConnection);
				cmd.ExecuteNonQuery();
				logger.Info("Пользователь удалён. Ok");
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка удаления пользователя!");
				throw ex;
			}
		}
	}
}
