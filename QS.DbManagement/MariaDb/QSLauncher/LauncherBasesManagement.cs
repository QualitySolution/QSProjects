using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherBasesManagement {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string LauncherBaseName = LauncherMetadataManagement.LauncherBaseName;
		private const string BasesTable = "bases";
		private static readonly string[] BaseDependencies = { "sessions", "api_tokens", "base_access" };

		private readonly bool canWrite;
		private readonly string connectionString;
		private readonly byte productId;
		private readonly int accountId;
		private readonly LauncherSchemaCache schema;

		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, int accountId, byte productId, LauncherSchemaCache schema) {
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;

			this.canWrite = canWrite;
			this.accountId = accountId;
			this.productId = productId;
			this.schema = schema ?? throw new ArgumentNullException(nameof(schema));
		}

		public int SyncBases() {
			if(!canWrite)
				throw new UnauthorizedAccessException($"У пользователя нет прав на запись в базу {LauncherBaseName}");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var bases = connection.Query<string>("SHOW DATABASES")
					.Except(MySqlSystemObjects.Databases, StringComparer.OrdinalIgnoreCase)
					.ToList();

				var tableColumns = schema.TableColumns(connection, LauncherBaseName, BasesTable);
				var keyColumns = schema.KeyColumns(connection, LauncherBaseName, BasesTable);

				var parameters = BaseParametersReader.ReadMany(connection, bases, BaseMetaParameters);

				var rows = new List<Dictionary<string, object>>();
				foreach(var dbName in bases) {
					var meta = ToBaseMeta(dbName, parameters);
					if(meta == null || meta.ProductCode != productId)
						continue;

					// значения, которые синхронизация умеет отдать, но в UpsertBases пойдут только те, что реально есть среди колонок таблицы
					rows.Add(new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
						["account_id"] = accountId,
						["product_id"] = meta.ProductCode,
						["base_name"] = dbName,
						["real_name"] = dbName, //?
						["base_title"] = meta.Title,
						["version"] = meta.Version,
						["base_guid"] = meta.Guid,
					});
				}

				int written = rows.Any() ? UpsertBases(connection, tableColumns, keyColumns, rows) : 0;

				// пропавшие с сервера базы помечаем disabled
				MarkMissingBasesDisabled(connection, tableColumns, bases);

				return written;
			}
		}

		private static int UpsertBases(MySqlConnection connection, IReadOnlyList<string> tableColumns, ICollection<string> keyColumns, IList<Dictionary<string, object>> rows, MySqlTransaction tx = null)
		{
			var columns = tableColumns.Where(rows[0].ContainsKey).ToList();
			if(!columns.Any())
				return 0;
			var updatable = columns.Where(c => !keyColumns.Contains(c)).ToList();

			const int chunkSize = 500;
			for(int offset = 0; offset < rows.Count; offset += chunkSize) {
				var chunk = rows.Skip(offset).Take(chunkSize).ToList();
				string sql = BuildUpsert(columns, updatable, chunk, out var parameters);
				connection.Execute(sql, parameters, tx);
			}

			return rows.Count;
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

			if(updatable.Any())
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
					bool deleted = SyncWithDelete(dbInfo, transaction);
					transaction.Commit();
					return deleted;
				}
			}
		}

		/// <summary>Транзакцию коммитит вызывающий</summary>
		public bool SyncWithDelete(DbInfo dbInfo, MySqlTransaction transaction)
		{
			// в удаление приходит и база, созданная только что при пересоздании: у неё известно лишь имя
			int baseId = dbInfo.BaseId > 0
				? dbInfo.BaseId
				: transaction.Connection.ExecuteScalar<int?>(
					$"SELECT id FROM `{BasesTable}` WHERE real_name = @name AND account_id = @acc AND product_id = @pid;",
					new { name = dbInfo.BaseName, acc = accountId, pid = productId }, transaction) ?? 0;

			if(baseId <= 0) {
				logger.Debug("База {0} в метабазе не значится, удалять нечего", dbInfo.BaseName);
				return false;
			}

			foreach(var dependency in BaseDependencies)
				transaction.Connection.Execute($"DELETE FROM `{dependency}` WHERE base_id = @id;",
					new { id = baseId }, transaction);
			transaction.Connection.Execute($"DELETE FROM `{BasesTable}` WHERE id = @id;", new { id = baseId }, transaction);

			logger.Info("Удалена база {0} аккаунтом {1}", dbInfo.BaseName, accountId);
			return true;
		}

		private static readonly string[] BaseMetaParameters = { "ProductCode", "version", "BaseTitle", "BaseGuid" };

		/// <summary>null - параметров базы нет либо в них нет кода продукта</summary>
		private static BaseMeta ToBaseMeta(string dbName, IReadOnlyDictionary<string, Dictionary<string, string>> byDatabase) {
			if(!byDatabase.TryGetValue(dbName, out var parameters))
				return null;

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
