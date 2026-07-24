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
		private LauncherUserInfo UserInfo;

		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite, string login, int productId) {
			connectionBuilder.Database = LauncherBaseName;
			connectionBuilder.AllowLoadLocalInfile = true;
			ConnectionString = connectionBuilder.ConnectionString;

			CanWrite = canWrite;
			UserInfo = GeLauncherUserInfo(login);

			ProductId = productId;
		}

		public void SyncBases(byte expectedProductCode) {
			if(!CanWrite)
				throw new UnauthorizedAccessException($"У пользователя нет прав на запись в базу {LauncherBaseName}");

			var items = new DataTable();

			using(MySqlConnection connection = new MySqlConnection(ConnectionString)) {
				connection.Open();

				var databases = connection.Query<string>("SHOW DATABASES").ToList();

				connection.Execute(@"
				 CREATE TEMPORARY TABLE bases_stage (
				     account_id INT,
				     product_id INT,
				     base_title VARCHAR(255),
				     base_name  VARCHAR(255),
				     version    VARCHAR(255)
				 )");

				(MySqlBulkCopy basesBulk, DataTable table) = GetBasesBulk(connection, "bases_stage");
				foreach(var dbName in databases.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase)) {
					byte? productCode = null;
					string version = null;
					string title = null;
					try {
						var rows = connection.Query<(string name, string str_value)>(
							$"SELECT name, str_value FROM `{dbName}`.base_parameters WHERE name IN ('ProductCode', 'version', 'BaseTitle')").ToList();
						foreach(var row in rows) {
							if(string.Equals(row.name, "ProductCode", StringComparison.OrdinalIgnoreCase))
								productCode = Convert.ToByte(row.str_value);
							else if(string.Equals(row.name, "version", StringComparison.OrdinalIgnoreCase))
								version = row.str_value;
							else if(string.Equals(row.name, "BaseTitle", StringComparison.OrdinalIgnoreCase))
								title = row.str_value;
						}
					}
					catch(MySqlException ex) {
						logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
						continue;
					}

					if((productCode != expectedProductCode))
						continue;

					table.Rows.Add(0, productCode, title, dbName, version);
				}
				using(var reader = table.CreateDataReader()) {
					var result = basesBulk.WriteToServer(reader);
					if(result.Warnings.Count > 0)
						throw new InvalidOperationException($"bulk copy warnings: {result.Warnings.Count}");
				}

				connection.Execute(@"
					INSERT INTO bases (account_id, product_id, base_title, base_name, version)
					SELECT account_id, product_id, base_title, base_name, version
					FROM bases_stage
					ON DUPLICATE KEY UPDATE
					    base_title = VALUES(base_title),
					    base_name  = VALUES(base_name),
					    version    = VALUES(version)");
			}
		}
		private (MySqlBulkCopy, DataTable) GetBasesBulk(MySqlConnection connection, string baseName)
		{
			var basesBulk = new MySqlBulkCopy(connection) { DestinationTableName = baseName };

			basesBulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(0, "account_id"));
			basesBulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(1, "product_id"));
			basesBulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(2, "base_title"));
			basesBulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(3, "base_name"));
			basesBulk.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(4, "version"));

			var table = new DataTable();
			table.Columns.Add("account_id", typeof(int));
			table.Columns.Add("product_id", typeof(int));
			table.Columns.Add("base_title", typeof(string));
			table.Columns.Add("base_name", typeof(string));
			table.Columns.Add("version", typeof(string));

			return (basesBulk, table);
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
						account_id = UserInfo.AccountId,
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
					"Удалена база {RealName} на сервере {Server} пользователем {UserId}",
					dbInfo.BaseName, UserInfo.Id);

				return true;
			}
		}

		private LauncherUserInfo GeLauncherUserInfo(string login) {
			using(var connection = new MySqlConnection(ConnectionString)) {
				var query = $@"SELECT `cloud_users`.`id` as Id, `cloud_users`.`login` as Login, `cloud_users`.`password` as PasswordHash, 
       				`accounts`.`id` as AccountId, accounts.login as AccountName, cloud_users.is_account_admin as IsAccountAdmin
					FROM `cloud_users` JOIN accounts ON accounts.id = `cloud_users`.`account_id` 
					WHERE `cloud_users`.`login` = @login;";
				var userInfo = connection.QueryFirstOrDefault<LauncherUserInfo>(query, new { login = login });
				if(userInfo == null)
					return null;

				return userInfo;
			}
		}
	}
}
