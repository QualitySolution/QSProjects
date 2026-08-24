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
		private static readonly string[] BaseDependencies = { "base_update_rights" };
		private static readonly string[] BaseColumns =
			{ "product_id", "base_name", "base_title", "version" };
		private static readonly string[] BaseUpdatableColumns = { "base_title", "version" };

		private readonly bool canSync;
		private readonly string connectionString;
		private readonly byte productId;


		/// <param name="canSync">
		/// Требуется полный обзор сервера
		/// </param>
		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canSync, byte productId) {
			var toLauncher = new MySqlConnectionStringBuilder(connectionBuilder.ConnectionString) {
				Database = LauncherBaseName,
				AllowLoadLocalInfile = true
			};
			connectionString = toLauncher.ConnectionString;

			this.canSync = canSync;
			this.productId = productId;
		}

		public int SyncBases() {
			if(!canSync)
				throw new UnauthorizedAccessException($"Синхронизировать {LauncherBaseName} может только пользователь с правами на весь сервер");

			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var bases = connection.Query<string>("SHOW DATABASES")
					.Except(MySqlSystemObjects.Databases, StringComparer.OrdinalIgnoreCase)
					.ToList();

				//Базы нашего продукта, о которых есть что записать в метабазу
				var parameters = BaseParametersReader.ReadMany(connection, bases, BaseMetaParameters);
				var rows = new List<BaseRow>();
				foreach(var dbName in bases)
				{
					BaseRow row = ToBaseMeta(dbName, parameters);
					if(row == null || row.ProductId != productId)
						continue;

					rows.Add(row);
				}

				int written = rows.Count > 0 ? UpsertBases(connection, rows) : 0;

				// пропавшие с сервера базы помечаем disabled
				MarkMissingBasesDisabled(connection, bases);

				return written;
			}
		}

		private static int UpsertBases(MySqlConnection connection, IList<BaseRow> rows, MySqlTransaction tx = null) {
			const int chunkSize = 500;
			for(int offset = 0; offset < rows.Count; offset += chunkSize) {
				var chunk = rows.Skip(offset).Take(chunkSize).ToList();
				string sql = BuildUpsert(chunk, out var parameters);
				connection.Execute(sql, parameters, tx);
			}

			return rows.Count;
		}

		/// <summary>Один INSERT со всеми строками пачки ON DUPLICATE KEY UPDATE</summary>
		private static string BuildUpsert(IReadOnlyList<BaseRow> chunk, out DynamicParameters parameters) {
			var sql = new StringBuilder($"INSERT INTO `{BasesTable}` (")
				.Append(string.Join(", ", BaseColumns.Select(c => $"`{c}`")))
				.Append(") VALUES ");
			parameters = new DynamicParameters();

			for(int row = 0; row < chunk.Count; row++) {
				if(row > 0)
					sql.Append(',');
				sql.Append('(')
					.Append(string.Join(",", BaseColumns.Select((c, i) => "@" + ParameterName(row, i))))
					.Append(')');

				var values = RowValues(chunk[row]);
				for(int i = 0; i < values.Length; i++)
					parameters.Add(ParameterName(row, i), values[i]);
			}

			sql.Append(" ON DUPLICATE KEY UPDATE ")
				.Append(string.Join(", ", BaseUpdatableColumns.Select(c => $"`{c}` = VALUES(`{c}`)")));

			return sql.ToString();
		}

		/// <summary>Порядок обязан совпадать с <see cref="BaseColumns"/></summary>
		private static object[] RowValues(BaseRow row) => new object[] {
			row.ProductId, row.BaseName, row.BaseTitle, row.Version
		};

		private static string ParameterName(int row, int column) => $"p{row}_{column}";

		private void MarkMissingBasesDisabled(MySqlConnection connection, IReadOnlyCollection<string> presentDatabases)
		{
			if(presentDatabases.Count == 0) {
				connection.Execute(
					$"UPDATE `{BasesTable}` SET disabled = TRUE WHERE product_id = @pid;",
					new { pid = productId });
				return;
			}
			// пропавшие -> disabled
			// вернувшиеся -> снимаем флаг
			connection.Execute(
				$"UPDATE `{BasesTable}` SET disabled = (base_name NOT IN @present) WHERE product_id = @pid;",
				new { present = presentDatabases, pid = productId });
		}

		public IEnumerable<DbInfo> GetBases()
		{
			using(var connection = new MySqlConnection(connectionString)) {
				connection.Open();

				var visible = new HashSet<string>(connection.Query<string>("SHOW DATABASES"),
					StringComparer.OrdinalIgnoreCase);

				return connection.Query<DbInfo>(
						"SELECT `id` AS BaseId, COALESCE(base_title, base_name) AS Title, " +
						"	`base_name` AS BaseName, `version` AS Version " +
						$"FROM `{BasesTable}` WHERE product_id = @productId " +
						"	AND base_name IN @visible;",
						new { productId, visible })
					.ToList();
			}
		}

		/// <summary>Идентификатор базы продукта в метабазе, 0 - её там нет</summary>
		public int FindBaseId(MySqlConnection connection, string baseName, MySqlTransaction transaction = null) =>
			connection.ExecuteScalar<int?>(
				$"SELECT id FROM `{BasesTable}` WHERE base_name = @name AND product_id = @pid;",
				new { name = baseName, pid = productId }, transaction) ?? 0;

		public int InsertBase(MySqlConnection connection, MySqlTransaction transaction, DbInfo dbInfo)
		{
			connection.Execute(
				"INSERT INTO bases (base_title, base_name, product_id) VALUES (@base_title, @base_name, @product_id);",
				new {
					base_title = dbInfo.Title,
					base_name = dbInfo.BaseName,
					product_id = productId,
				}, transaction);

			return connection.ExecuteScalar<int>("SELECT LAST_INSERT_ID();", transaction: transaction);
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
				: FindBaseId(transaction.Connection, dbInfo.BaseName, transaction);

			if(baseId <= 0) {
				logger.Debug("База {0} в метабазе не значится, удалять нечего", dbInfo.BaseName);
				return false;
			}

			foreach(var dependency in BaseDependencies)
				transaction.Connection.Execute($"DELETE FROM `{dependency}` WHERE base_id = @id;",
					new { id = baseId }, transaction);
			transaction.Connection.Execute($"DELETE FROM `{BasesTable}` WHERE id = @id;", new { id = baseId }, transaction);

			logger.Info("Удалена база {0} продукта {1}", dbInfo.BaseName, productId);
			return true;
		}

		private static readonly string[] BaseMetaParameters = { "ProductCode", "version", "BaseTitle" };

		/// <summary>null - параметров базы нет либо в них нет кода продукта</summary>
		private static BaseRow ToBaseMeta(string dbName, IReadOnlyDictionary<string, Dictionary<string, string>> byDatabase) {
			if(!byDatabase.TryGetValue(dbName, out var parameters))
				return null;

			if(!parameters.TryGetValue("ProductCode", out var code) || !byte.TryParse(code, out var productCode))
				return null;

			return new BaseRow
			{
				ProductId = productCode,
				Version = Parameter(parameters, "version"),
				BaseTitle = Parameter(parameters, "BaseTitle")
			};
		}

		private static string Parameter(IReadOnlyDictionary<string, string> parameters, string name)
			=> parameters.TryGetValue(name, out var value) ? value : null;

		/// <summary>Строка таблицы bases, как её пишет синхронизация</summary>
		private sealed class BaseRow {
			public byte ProductId { get; set; }
			public string BaseName { get; set; }
			public string BaseTitle { get; set; }
			public string Version { get; set; }
		}
	}
}
