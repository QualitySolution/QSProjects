using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class BaseUsersManagement {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string UsersTable = "users";
		// id - первичный ключ, login - идентификатор для upsert
		private static readonly HashSet<string> StructuralColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", "login" };

		private readonly string connectionString;

		public BaseUsersManagement(MySqlConnectionStringBuilder connectionBuilder) {
			connectionString = connectionBuilder.ConnectionString;
		}

		/// <summary>Заводит, обновляет пользователя в базе при доступе, снимает доступ через deactivated</summary>
		public void SyncWithUserTable(string baseName, BaseUserRow user, bool hasAccess) {
			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var columns = LauncherColumnMapper.TableColumns(connection, baseName, UsersTable);
					if(!columns.Any())
						return; // таблицы users в этой базе нет

					string table = $"`{MySqlEscape.Identifier(baseName)}`.`{UsersTable}`";

					if(!hasAccess) {
						if(columns.Contains("deactivated", StringComparer.OrdinalIgnoreCase))
						{
							connection.Execute($"UPDATE {table} SET deactivated = TRUE WHERE login = @login", new { login = user.Login });
						}
						else {
							logger.Warn("в базе {0} нет таблицы {1}", baseName, UsersTable);
						}
						return;
					}

					// name NOT NULL без дефолта - гарантируем значение на уровне сущности
					if(string.IsNullOrEmpty(user.Name))
						user.Name = user.Login;

					var (cols, parameters) = LauncherColumnMapper.MapForWrite(columns, user, StructuralColumns);
					bool exists = connection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM {table} WHERE login = @login", new { login = user.Login }) > 0;
					parameters.Add("login", user.Login);

					if(exists) {
						// пустые поля формы не затирают существующее
						var setParts = cols.Select(c => $"`{c}` = COALESCE(@{c}, `{c}`)");
						connection.Execute($"UPDATE {table} SET {string.Join(", ", setParts)} WHERE login = @login", parameters);
					}
					else {
						cols.Insert(0, "login");
						connection.Execute(
							$"INSERT INTO {table} ({string.Join(", ", cols.Select(c => $"`{c}`"))}) " +
							$"VALUES ({string.Join(", ", cols.Select(c => "@" + c))})", parameters);
					}
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось синхронизировать users в базе {0} для {1}", baseName, user.Login);
			}
		}

		public void SyncWithDeletingUser(string login, List<string> baseNames)
		{
			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					List<string> sqls = new List<string>(baseNames.Count);
					foreach(string baseName in baseNames) {
						List<string> columns = LauncherColumnMapper.TableColumns(connection, baseName, UsersTable);
						if(!columns.Any())
							continue; // таблицы users в этой базе нет

						string table = $"`{MySqlEscape.Identifier(baseName)}`.`{UsersTable}`";

						if(columns.Contains("deactivated", StringComparer.OrdinalIgnoreCase)) {
							sqls.Add($"UPDATE {table} SET deactivated = TRUE WHERE login = @login");
						}
						else {
							logger.Warn("в базе {0} нет таблицы {1}", baseName, UsersTable);
						}
						return;
					}
					if(!sqls.Any())
						return;
					connection.Execute(string.Join("; ", sqls), new { login = login });
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось синхронизировать отключение пользователя {0} из всех таблиц users", login);
			}
		}

		/// <summary>Профиль пользователя из таблицы базы</summary>
		public BaseUserRow TryGetProfile(string baseName, string login) {
			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var columns = LauncherColumnMapper.TableColumns(connection, baseName, UsersTable);
					if(!columns.Any())
						return null;
					string select = LauncherColumnMapper.SelectList(columns, typeof(BaseUserRow));
					return connection.QueryFirstOrDefault<BaseUserRow>(
						$"SELECT {select} FROM `{MySqlEscape.Identifier(baseName)}`.`{UsersTable}` WHERE login = @login", new { login });
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось прочитать users в базе {0}", baseName);
				return null;
			}
		}
	}
}
