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

		private const string LauncherBaseName = "QSLauncher";
		private static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };
		static readonly string[] BaseColumns = { "account_id", "product_id", "base_title", "base_name", "version" };
		static readonly string[] BaseDependencies = { "sessions", "api_tokens", "base_access", "bases" }; // базы последнии

		private bool CanWrite;
		private string ConnectionString;
		private int ProductId;
		private string AccountId;

		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, string accountId, int productId) {
			connectionBuilder.Database = LauncherBaseName;
			connectionBuilder.AllowLoadLocalInfile = true;
			ConnectionString = connectionBuilder.ConnectionString;

			CanWrite = canWrite;
			AccountId = accountId;
			ProductId = productId;
		}

		public void SyncBases(byte expectedProductCode) {
			if(!CanWrite)
				throw new UnauthorizedAccessException($"У пользователя нет прав на запись в базу {LauncherBaseName}");

			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();

				var databases = connection.Query<string>("SHOW DATABASES")
					.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase)
					.ToList();

				var rows = new List<BaseRow>();

				foreach(var dbName in databases) {
					byte? productCode = null;
					string version = null;
					string title = null;
					try {
						var parameters = connection.Query(
							$"SELECT name, str_value FROM `{dbName}`.base_parameters WHERE name IN ('ProductCode', 'version', 'BaseTitle')");
						foreach(var row in parameters) {
							string name = row.name;
							string value = row.str_value;
							if(string.Equals(name, "ProductCode", StringComparison.OrdinalIgnoreCase))
								productCode = Convert.ToByte(value);
							else if(string.Equals(name, "version", StringComparison.OrdinalIgnoreCase))
								version = value;
							else if(string.Equals(name, "BaseTitle", StringComparison.OrdinalIgnoreCase))
								title = value;
						}
					}
					catch(MySqlException ex) {
						logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
						continue;
					}

					if(productCode != expectedProductCode)
						continue;

					rows.Add(new BaseRow(0, productCode.Value, title, dbName, version));
				}

				if(rows.Count == 0)
					return;

				UpsertBases(connection, rows);
			}
		}
		private void UpsertBases(MySqlConnection connection, IList<BaseRow> rows, MySqlTransaction tx = null) {
			const int chunkSize = 500;

			for(int offset = 0; offset < rows.Count; offset += chunkSize) {
				var chunk = rows.Skip(offset).Take(chunkSize).ToList();

				var sb = new StringBuilder(
					"INSERT INTO bases (account_id, product_id, base_title, base_name, version) VALUES ");
				var p = new DynamicParameters();

				for(int i = 0; i < chunk.Count; i++) {
					object[] values = { chunk[i].AccountId, chunk[i].ProductId, chunk[i].Title, chunk[i].Name, chunk[i].Version }; // для расширения и универсальности надо этот массив определять вне

					sb.Append(i > 0 ? ",(" : "(");
					for(int c = 0; c < values.Length; c++) {
						string key = $"p{i}_{c}";
						if(c > 0) sb.Append(',');
						sb.Append('@').Append(key);
						p.Add(key, values[c]);
					}
					sb.Append(')');
				}

				sb.Append(@" ON DUPLICATE KEY UPDATE
					base_title = VALUES(base_title),
					base_name  = VALUES(base_name),
					version    = VALUES(version)");

				connection.Execute(sb.ToString(), p, tx);
			}
		}

		public IEnumerable<DbInfo> GetBases(string login) {
			using(MySqlConnection connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				var sql = @"
					SELECT `base_id` AS BaseId, COALESCE(base_title, base_name) AS Title, COALESCE(real_name, '') AS BaseName, version AS Version
					FROM `base_access`
					JOIN `bases` ON `base_access`.`base_id` = `bases`.`id`
					JOIN `server_users` ON `base_access`.`user_id` = `server_users`.`id`
					WHERE `server_users`.`login`= @login
						AND `bases`.`product_id` = @productId;";
				return connection.Query<DbInfo>(sql, new { login, ProductId });
			}
		}

		public (int, string) SyncWithCreation(DbInfo dbInfo) {
			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				var baseGuid = Guid.NewGuid().ToString();

				using(var transaction = connection.BeginTransaction()) {

					var insertBaseSql =
						"INSERT INTO bases (account_id, base_title, base_name, product_id, real_name, base_guid) " +
						"VALUES (@account_id, @base_title, @base_name, @product_id, @real_name, @base_guid);";
					connection.Execute(insertBaseSql, new {
						account_id = AccountId,
						base_title = dbInfo.Title,
						base_name = dbInfo.BaseName,
						product_id = ProductId,
						real_name = dbInfo.BaseName,
						base_guid = baseGuid,
					}, transaction);

					var baseId = connection.ExecuteScalar<int>(
						"SELECT LAST_INSERT_ID();", transaction: transaction);

					connection.Execute(
						"INSERT INTO base_access (user_id, base_id, admin) " +
						"VALUES (@user_id, @base_id, 1);",
						new { user_id = UserInfo.Id, base_id = baseId }, transaction); //может вынести в LauncherUsersManagement

					transaction.Commit();
					return (baseId, baseGuid);
				}
			}
		}

		public bool SyncWithDelete(DbInfo dbInfo) {
			using(var connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				var sb = new StringBuilder();
				foreach(var dependency in BaseDependencies)
					sb.Append($"DELETE FROM {dependency} WHERE base_id = @id; ");

				connection.Execute(sb.ToString(), new { id = dbInfo.BaseId });

				logger.Info(
					"Удалена база {RealName} аккаунтом {UserId}",
					dbInfo.BaseName, AccountId);

				return true;
			}
		}
	}
}
