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

		// незаполненное поле формы приходит null и через COALESCE не затирает то,
		// что в базу вписало само приложение
		private const string ProfileSet = "`name` = COALESCE(@name, `name`), `email` = COALESCE(@email, `email`)";
		private const string ProfileSelect = "`name` AS `Name`, `email` AS `Email`";

		private readonly string connectionString;

		public BaseUsersManagement(MySqlConnectionStringBuilder connectionBuilder) {
			connectionString = connectionBuilder.ConnectionString;
		}

		/// <summary>Заводит, обновляет пользователя в базе при доступе, снимает доступ через deactivated</summary>
		public void SyncWithUserTable(string baseName, BaseUserRow user, bool hasAccess) {
			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					if(!MySqlMultiBase.HasTable(connection, baseName, UsersTable))
						return; // таблицы users в этой базе нет

					string table = Table(baseName);
					if(!hasAccess) {
						connection.Execute($"UPDATE {table} SET `deactivated` = TRUE WHERE `login` = @login",
							new { login = user.Login });
						return;
					}

					bool exists = connection.ExecuteScalar<bool>(
						$"SELECT COUNT(*) > 0 FROM {table} WHERE `login` = @login", new { login = user.Login });

					// колонка name в базах NOT NULL - без подстановки вставка упадёт
					if(!exists && string.IsNullOrEmpty(user.Name))
						user.Name = user.Login;

					connection.Execute(exists
							? $"UPDATE {table} SET {ProfileSet}, `admin` = @admin, `deactivated` = FALSE WHERE `login` = @login"
							: $"INSERT INTO {table} (`login`, `name`, `email`, `admin`, `deactivated`) " +
								"VALUES (@login, @name, @email, @admin, FALSE)",
						new { login = user.Login, name = Blank(user.Name), email = Blank(user.Email), admin = user.Admin });
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось синхронизировать users в базе {0} для {1}", baseName, user.Login);
			}
		}

		public void SyncWithDeletingUser(string login, List<string> baseNames) {
			try {
				UpdateInBases(baseNames, "`deactivated` = TRUE", new { login });
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось синхронизировать отключение пользователя {0} из всех таблиц users", login);
			}
		}

		public void SyncProfile(IEnumerable<string> baseNames, string login, string name, string email) {
			if(string.IsNullOrEmpty(name) && string.IsNullOrEmpty(email))
				return;

			try {
				UpdateInBases(baseNames, ProfileSet, new { login, name = Blank(name), email = Blank(email) });
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "не удалось обновить профиль пользователя {0} в таблицах {1}", login, UsersTable);
			}
		}

		public Dictionary<string, BaseUserRow> TryGetProfiles(IEnumerable<string> baseNames, string login) {
			var result = new Dictionary<string, BaseUserRow>(StringComparer.OrdinalIgnoreCase);
			var wanted = Wanted(baseNames);
			if(wanted.Count == 0)
				return result;

			try {
				using(var connection = new MySqlConnection(connectionString)) {
					connection.Open();
					var parameters = new DynamicParameters();
					parameters.Add("login", login);

					// набор колонок один на все базы, поэтому ветки UNION соединяются как есть
					var projections = MySqlMultiBase.DatabasesWithTable(connection, wanted, UsersTable)
						.Select(baseName => new KeyValuePair<string, string>(baseName, ProfileSelect));
					string sql = MySqlMultiBase.UnionAll(projections, UsersTable,
						nameof(BaseProfileRow.BaseName), "`login` = @login", parameters);
					if(sql.Length == 0)
						return result;

					foreach(BaseProfileRow row in connection.Query<BaseProfileRow>(sql, parameters))
						result[row.BaseName] = row;
				}
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось прочитать {0} по базам пользователя {1}", UsersTable, login);
			}
			return result;
		}

		/// <summary>Все базы одним запросом: поштучно это два запроса на каждую</summary>
		private void UpdateInBases(IEnumerable<string> baseNames, string setClause, object parameters) {
			var wanted = Wanted(baseNames);
			if(wanted.Count == 0)
				return;

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var sqls = MySqlMultiBase.DatabasesWithTable(connection, wanted, UsersTable)
					.Select(baseName => $"UPDATE {Table(baseName)} SET {setClause} WHERE `login` = @login")
					.ToList();
				if(sqls.Count == 0)
					return;

				connection.Execute(string.Join("; ", sqls), parameters);
			}
		}

		private static List<string> Wanted(IEnumerable<string> baseNames)
			=> MySqlMultiBase.Distinct(baseNames ?? Enumerable.Empty<string>());

		private static string Table(string baseName)
			=> $"`{MySqlEscape.Identifier(baseName)}`.`{UsersTable}`";

		/// <summary>Пустую строку из формы кладём как NULL</summary>
		private static string Blank(string value) => string.IsNullOrEmpty(value) ? null : value;

		private sealed class BaseProfileRow : BaseUserRow {
			public string BaseName { get; set; }
		}
	}
}
