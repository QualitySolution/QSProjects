using Dapper;
using MySqlConnector;
using QS.BaseParameters;
using QS.DbManagement.Entities;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherBasesManagement {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public const string LauncherBaseName = "QSLauncher";
		private const string BasesTable = "bases";
		public static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };
		private static readonly string[] BaseDependencies = { "sessions", "api_tokens", "base_access" };

		private readonly bool canWrite;
		private readonly string connectionString;
		private readonly int productId;
		private readonly int accountId;

		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, int accountId, int productId) {
			connectionBuilder.Database = LauncherBaseName;
			connectionBuilder.AllowLoadLocalInfile = true;
			connectionString = connectionBuilder.ConnectionString;

			this.canWrite = canWrite;
			this.accountId = accountId;
			this.productId = productId;
		}

		public void SyncBases() {
			if(!canWrite)
				throw new UnauthorizedAccessException($"У пользователя нет прав на запись в базу {LauncherBaseName}");

			byte expectedProductCode = (byte)productId;

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var bases = connection.Query<string>("SHOW DATABASES")
					.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase);

				var tableColumns = LauncherColumnMapper.TableColumns(connection, LauncherBaseName, BasesTable);
				var keyColumns = LauncherColumnMapper.KeyColumns(connection, LauncherBaseName, BasesTable);

				var rows = new List<Dictionary<string, object>>();
				foreach(var dbName in bases) {
					var meta = ReadBaseParameters(dbName);
					if(meta == null || meta.ProductCode != expectedProductCode)
						continue;

					// значения, которые синхронизация умеет отдать; в UpsertBases пойдут только те,
					// что реально есть среди колонок таблицы
					rows.Add(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { //?
						["account_id"] = accountId,
						["product_id"] = meta.ProductCode,
						["base_name"] = dbName,
						["real_name"] = dbName,
						["base_title"] = meta.Title,
						["version"] = meta.Version,
						["base_guid"] = meta.Guid,
					});
				}

				if(rows.Any())
					UpsertBases(connection, tableColumns, keyColumns, rows);

				// пропавшие с сервера базы помечаем disabled (мягкое удаление при синхронизации)
				MarkMissingBasesDisabled(connection, tableColumns, bases);
			}
		}

		private static void UpsertBases(MySqlConnection connection, IReadOnlyList<string> tableColumns, ICollection<string> keyColumns, IList<Dictionary<string, object>> rows, MySqlTransaction tx = null)
		{
			var columns = tableColumns.Where(rows[0].ContainsKey).ToList();
			if(columns.Count == 0)
				return;
			var updatable = columns.Where(c => !keyColumns.Contains(c)).ToList();

			const int chunkSize = 500;
			for(int offset = 0; offset < rows.Count; offset += chunkSize) {
				var chunk = rows.Skip(offset).Take(chunkSize).ToList();
				string sql = BuildUpsert(columns, updatable, chunk, out var parameters);
				connection.Execute(sql, parameters, tx);
			}
		}

		/// <summary>Один INSERT со всеми строками пачки: VALUES (…),(…) плюс ON DUPLICATE KEY UPDATE.</summary>
		private static string BuildUpsert(IReadOnlyList<string> columns, IReadOnlyList<string> updatable,
			IReadOnlyList<Dictionary<string, object>> chunk, out DynamicParameters parameters) {
			var sql = new StringBuilder($"INSERT INTO `{BasesTable}` (")
				.Append(string.Join(", ", columns.Select(c => $"`{c}`")))
				.Append(") VALUES ");
			parameters = new DynamicParameters();

			for(int row = 0; row < chunk.Count; row++) {
				if(row > 0)
					sql.Append(',');
				sql.Append('(')
					.Append(string.Join(",", columns.Select((c, i) => "@" + ParameterName(row, i))))
					.Append(')');

				for(int i = 0; i < columns.Count; i++)
					parameters.Add(ParameterName(row, i), chunk[row].TryGetValue(columns[i], out var value) ? value : null);
			}

			if(updatable.Count > 0)
				sql.Append(" ON DUPLICATE KEY UPDATE ")
					.Append(string.Join(", ", updatable.Select(c => $"`{c}` = VALUES(`{c}`)")));

			return sql.ToString();
		}

		private static string ParameterName(int row, int column) => $"p{row}_{column}";

		private void MarkMissingBasesDisabled(MySqlConnection connection, ICollection<string> tableColumns, IEnumerable<string> presentDatabases)
		{
			if(!tableColumns.Contains("disabled", StringComparer.OrdinalIgnoreCase))
				return;

			if(!presentDatabases.Any()) {
				connection.Execute(
					$"UPDATE `{BasesTable}` SET disabled = TRUE WHERE account_id = @acc AND product_id = @pid;",
					new { acc = accountId, pid = productId });
				return;
			}
			// пропавшие -> disabled
			// вернувшиеся -> снимаем флаг
			connection.Execute(
				$"UPDATE `{BasesTable}` SET disabled = (base_name NOT IN @present) WHERE account_id = @acc AND product_id = @pid;",
				new { present = presentDatabases, acc = accountId, pid = productId });
		}

		public IEnumerable<DbInfo> GetBases(string login)
		{
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				var sql = @"
					SELECT `bases`.`id` AS BaseId, COALESCE(base_title, base_name) AS Title,
						COALESCE(real_name, base_name, '') AS BaseName, version AS Version
					FROM `base_access`
					JOIN `bases` ON `base_access`.`base_id` = `bases`.`id`
					JOIN `server_users` ON `base_access`.`user_id` = `server_users`.`id`
					WHERE `server_users`.`login` = @login
						AND `bases`.`product_id` = @productId;";
				return connection.Query<DbInfo>(sql, new { login, productId }).ToList();
			}
		}

		public (int baseId, string baseGuid) InsertBase(MySqlConnection connection, MySqlTransaction transaction, DbInfo dbInfo)
		{
			var baseGuid = Guid.NewGuid().ToString();
			connection.Execute(
				"INSERT INTO bases (account_id, base_title, base_name, product_id, real_name, base_guid) " +
				"VALUES (@account_id, @base_title, @base_name, @product_id, @real_name, @base_guid);",
				new {
					account_id = accountId,
					base_title = dbInfo.Title,
					base_name = dbInfo.BaseName,
					product_id = productId,
					real_name = dbInfo.BaseName,
					base_guid = baseGuid,
				}, transaction);

			var baseId = connection.ExecuteScalar<int>("SELECT LAST_INSERT_ID();", transaction: transaction);
			return (baseId, baseGuid);
		}

		public bool SyncWithDelete(DbInfo dbInfo)
		{
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();
				using(var transaction = connection.BeginTransaction()) {
					return SyncWithDelete(dbInfo, transaction);
				}
			}
		}

		public bool SyncWithDelete(DbInfo dbInfo, MySqlTransaction transaction)
		{
			foreach(var dependency in BaseDependencies)
				transaction.Connection.Execute($"DELETE FROM `{dependency}` WHERE base_id = @id;",
					new { id = dbInfo.BaseId }, transaction);
			transaction.Connection.Execute($"DELETE FROM `{BasesTable}` WHERE id = @id;", new { id = dbInfo.BaseId }, transaction);
			transaction.Commit();
			logger.Info("Удалена база {0} аккаунтом {1}", dbInfo.BaseName, accountId);
			return true;
		}

		private BaseMeta ReadBaseParameters(string dbName) {
			Dictionary<string, string> parameters;
			try {
				var toBase = new MySqlConnectionStringBuilder(connectionString) { Database = dbName };
				parameters = new ParametersService(new MySqlConnectionFactory(toBase.ConnectionString).OpenConnection).All;
			}
			catch(MySqlException ex) {
				logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
				return null;
			}

			if(!parameters.TryGetValue("ProductCode", out var code) || !byte.TryParse(code, out var productCode))
				return null;

			return new BaseMeta {
				ProductCode = productCode,
				Version = Parameter(parameters, "version"),
				Title = Parameter(parameters, "BaseTitle"),
				Guid = Parameter(parameters, "BaseGuid")
			};
		}

		private static string Parameter(IReadOnlyDictionary<string, string> parameters, string name)
			=> parameters.TryGetValue(name, out var value) ? value : null;

		private sealed class BaseMeta {
			public byte ProductCode { get; set; }
			public string Version { get; set; }
			public string Title { get; set; }
			public string Guid { get; set; }
		}
	}
}
