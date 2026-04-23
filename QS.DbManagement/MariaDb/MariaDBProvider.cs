using Dapper;
using MySqlConnector;
using QS.DbManagement.Responces;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace QS.DbManagement
{
	public class MariaDBProvider : IDbProvider {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly string[] SystemDatabases = { "information_schema", "mysql", "performance_schema", "sys" };

		readonly MySqlConnection connection;
		readonly MySqlConnectionStringBuilder connectionStringBuilder;

		public bool IsConnected => connection.State == ConnectionState.Open;

		public bool IsAdmin { get; private set; }

		/// <summary>
		/// Есть ли у текущего пользователя право создавать базы данных.
		/// Определяется в момент <see cref="LoginToServer"/> из SHOW GRANTS.
		/// </summary>
		public bool CanCreateDatabase { get; private set; }

		#region Параметры подключения
		public string Server { get; }
		public string UserName { get; }
		public string ProductName { get; }
		private readonly string password;
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			Server = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			ProductName = parameters.First(p => p.Name == "ProductName").Value;
			this.password = password;

			var builder = new MySqlConnectionStringBuilder {
				Server = Server,
				UserID = UserName,
				Password = password,
				AllowUserVariables = true,
				ConvertZeroDateTime = true
			};
			connectionStringBuilder = builder;
			connection = new MySqlConnection(builder.ConnectionString);
		}

		#region IDbProvider

		public LoginToServerResponse LoginToServer() {
			try {
				if(connection.State != ConnectionState.Open)
					connection.Open();

				var grants = connection.Query<string>("SHOW GRANTS FOR CURRENT_USER").ToList();

				IsAdmin = grants.Any(g =>
					g.IndexOf("ALL PRIVILEGES ON *.*", StringComparison.OrdinalIgnoreCase) >= 0
					|| g.IndexOf("GRANT OPTION", StringComparison.OrdinalIgnoreCase) >= 0
					|| g.IndexOf("SUPER", StringComparison.OrdinalIgnoreCase) >= 0);

				CanCreateDatabase = IsAdmin || grants.Any(g =>
					g.IndexOf("ALL PRIVILEGES", StringComparison.OrdinalIgnoreCase) >= 0
					|| g.IndexOf("CREATE", StringComparison.OrdinalIgnoreCase) >= 0); 
				//todo : разбить на токены и одним циклом по массиву прав проставить булы

				return new LoginToServerResponse {
					Success = true,
					IsAdmin = IsAdmin,
					NeedToUpdateLauncher = false
				};
			}
			catch(MySqlException ex) {
				logger.Warn(ex, "Не удалось подключиться к MariaDB {0} как {1}", Server, UserName);
				return new LoginToServerResponse {
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		public List<DbInfo> GetUserDatabases(IApplicationInfo applicationInfo) {
			var result = new List<DbInfo>();

			if(connection.State != ConnectionState.Open)
				connection.Open();

			var databases = connection.Query<string>("SHOW DATABASES").ToList();
			var expectedProductName = ProductName;

			foreach(var dbName in databases.Except(SystemDatabases, StringComparer.OrdinalIgnoreCase)) {
				var tableExists = connection.QueryFirstOrDefault<string>(
					$"SHOW TABLES IN `{dbName}` LIKE 'base_parameters'") != null;

				if(!tableExists)
					continue;

				string productName = null;
				string version = null;
				try {
					var rows = connection.Query<(string name, string str_value)>(
						$"SELECT name, str_value FROM `{dbName}`.base_parameters WHERE name IN ('product_name', 'version')").ToList();
					foreach(var row in rows) {
						if(string.Equals(row.name, "product_name", StringComparison.OrdinalIgnoreCase))
							productName = row.str_value;
						else if(string.Equals(row.name, "version", StringComparison.OrdinalIgnoreCase))
							version = row.str_value;
					}
				}
				catch(MySqlException ex) {
					logger.Debug(ex, "Не удалось прочитать base_parameters в базе {0}", dbName);
					continue;
				}

				if(!string.IsNullOrEmpty(expectedProductName)
					&& !string.Equals(productName, expectedProductName, StringComparison.OrdinalIgnoreCase))
					continue;

				result.Add(new DbInfo {
					BaseName = dbName,
					Title = dbName,
					Version = version,
					CanCreateDatabase = CanCreateDatabase
				});
			}

			return result;
		}

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			try {
				connectionStringBuilder.Database = dbInfo.BaseName;

				return new LoginToDatabaseResponse {
					Success = true,
					ConnectionString = connectionStringBuilder.ConnectionString,
					Login = UserName,
					Parameters = new Dictionary<string, string> {
						{ "BaseTitle", dbInfo.Title },
						{ "SessionId", string.Empty }	
					}
				};
			}
			catch(Exception ex) {
				return new LoginToDatabaseResponse {
					Success = false,
					ErrorMessage = ex.Message
				};
			}
		}

		public bool AddUser(string username, string password) {
			string sql = $"CREATE USER IF NOT EXISTS '{username}' IDENTIFIED BY '{password}'";
			return connection.Execute(sql) != 0;
		}

		public bool ChangePassword(string username, string oldPassword, string newPassword) {
			string sql = $"ALTER USER '{username}'@'%' IDENTIFIED BY '{newPassword}'";
			return connection.Execute(sql) != 0;
		}

		public bool CreateDatabase(string databaseName) {
			string sql = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
			return connection.Execute(sql) != 0;
		}

		public bool DropDatabase(string databaseName) {
			string sql = $"DROP DATABASE IF EXISTS `{databaseName}`";
			return connection.Execute(sql) != 0;
		}

		public void Dispose() {
			connection?.Dispose();
		}

		#endregion
	}
}
