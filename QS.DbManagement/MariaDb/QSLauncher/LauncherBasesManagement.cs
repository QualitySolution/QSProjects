using Dapper;
using MySqlConnector;
using QS.DbManagement.Entities;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QS.DbManagement.MariaDb.QSLauncher {
	internal class LauncherBasesManagement
	{
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private const string LauncherBaseName = "QSLauncher";
		private static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };

		private bool CanWrite;
		private bool GlobalAdmin;
		private string ConnectionString;

		public LauncherBasesManagement(MySqlConnectionStringBuilder connectionBuilder, bool canWrite) {
			connectionBuilder.Database = LauncherBaseName;
			connectionBuilder.AllowLoadLocalInfile = true;
			ConnectionString = connectionBuilder.ConnectionString;

			CanWrite = canWrite;
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
				     version    INT
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
					var result = basesBulk.WriteToServer(items);
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

		public IEnumerable<DbInfo> GetBases(string login, int productId) {
			using(MySqlConnection connection = new MySqlConnection(ConnectionString)) {
				connection.Open();
				var sql = @"
					SELECT `base_id` AS BaseId, COALESCE(base_title, base_name) AS Title, COALESCE(real_name, '') AS BaseName, version AS Version
					FROM `base_access`
					JOIN `bases` ON `base_access`.`base_id` = `bases`.`id`
					JOIN `server_users` ON `base_access`.`user_id` = `server_users`.`id`
					WHERE `server_users`.`login`= @login
						AND `bases`.`product_id` = @productId;";
				return connection.Query<DbInfo>(sql, new { login, productId });
			}
		}
	}
}
