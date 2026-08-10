using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

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
							logger.Warn("в базе {0} у таблицы {1} нет столбца deactivated", baseName, UsersTable);
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
							logger.Warn("в базе {0} у таблицы {1} нет столбца deactivated", baseName, UsersTable);
						}
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

		private static readonly string[] ProfileColumns = { "name", "email" };

		public void SyncProfile(IEnumerable<string> baseNames, string login, string name, string email) {
			var wanted = baseNames?.Where(b => !string.IsNullOrEmpty(b)).ToList();
			if(wanted == null || wanted.Count == 0)
				return;
			if(string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
				return;

			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var sqls = MySqlMultiBase.TableColumns(connection, wanted, UsersTable)
						.Select(pair => ProfileUpdate(pair.Key, pair.Value))
						.Where(sql => sql != null)
						.ToList();
					if(sqls.Count == 0)
						return;

					// незаполненное поле формы приходит null и через COALESCE не затирает базу
					connection.Execute(string.Join("; ", sqls), new {
						login,
						name = string.IsNullOrEmpty(name) ? null : name,
						email = string.IsNullOrEmpty(email) ? null : email
					});
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось обновить профиль пользователя {0} в таблицах {1}", login, UsersTable);
			}
		}

		/// <summary>null - в этой базе профильных колонок нет</summary>
		private static string ProfileUpdate(string baseName, ICollection<string> columns) {
			var setParts = ProfileColumns
				.Where(c => columns.Contains(c, StringComparer.OrdinalIgnoreCase))
				.Select(c => $"`{c}` = COALESCE(@{c}, `{c}`)")
				.ToList();
			if(setParts.Count == 0)
				return null;

			return $"UPDATE `{MySqlEscape.Identifier(baseName)}`.`{UsersTable}` " +
				$"SET {string.Join(", ", setParts)} WHERE login = @login";
		}

		public Dictionary<string, BaseUserRow> TryGetProfiles(IEnumerable<string> baseNames, string login) {
			var result = new Dictionary<string, BaseUserRow>(StringComparer.OrdinalIgnoreCase);
			var wanted = baseNames?.Where(b => !string.IsNullOrEmpty(b)).ToList();
			if(wanted == null || !wanted.Any())
				return result;

			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var columnsByBase = MySqlMultiBase.TableColumns(connection, wanted, UsersTable);
					if(!columnsByBase.Any())
						return result;

					ReadProfiles(connection, columnsByBase, login, result);
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось прочитать {0} по базам пользователя {1}", UsersTable, login);
			}
			return result;
		}

		private static void ReadProfiles(MySqlConnection connection, Dictionary<string, List<string>> columnsByBase,
			string login, IDictionary<string, BaseUserRow> result) {
			var parameters = new DynamicParameters();
			parameters.Add("login", login);

			// набор колонок в каждой базе свой, поэтому проекция считается для каждой отдельно:
			// выровненная, иначе ветки UNION не соединить
			var projections = columnsByBase.Select(pair => new KeyValuePair<string, string>(
				pair.Key, LauncherColumnMapper.SelectListAligned(pair.Value, typeof(BaseUserRow))));
			string sql = MySqlMultiBase.UnionAll(projections, UsersTable,
				nameof(BaseProfileRow.BaseName), "login = @login", parameters);

			IEnumerable<BaseProfileRow> response = connection.Query<BaseProfileRow>(sql, parameters);
			foreach(BaseProfileRow row in response)
				result[row.BaseName] = row;
		}

		private sealed class BaseProfileRow : BaseUserRow {
			public string BaseName { get; set; }
		}

	}
}
