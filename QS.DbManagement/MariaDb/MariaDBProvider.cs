using Dapper;
using MySqlConnector;
using QS.DbManagement.Responces;
using QS.Project.Versioning;
using System;
using System.Collections.Generic;
using System.Data;
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
		private readonly string password;
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, string password = null) {
			if(parameters == null)
				throw new ArgumentNullException(nameof(parameters));

			string serverValue = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			this.password = password;

			string host = serverValue;
			uint? port = null;
			if(serverValue.Contains(":")) {
				var parts = serverValue.Split(':');
				host = parts[0];
				if(uint.TryParse(parts[1], out var parsedPort))
					port = parsedPort;
			}
			Server = serverValue;

			connectionStringBuilder = new MySqlConnectionStringBuilder {
				Server = host,
				UserID = UserName,
				Password = password,
				AllowUserVariables = true
			};
			if(port != null)
				connectionStringBuilder.Port = port.Value;
			connection = new MySqlConnection(connectionStringBuilder.ConnectionString);
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

				return new LoginToServerResponse {
					Success = true,
					IsAdmin = IsAdmin,
					NeedToUpdateLauncher = false,
					CanCreateDatabase = CanCreateDatabase
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
			byte expectedProductCode = applicationInfo.ProductCode;

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

				if((expectedProductCode != null)
					&& (productCode != expectedProductCode))
					continue;

				result.Add(new DbInfo {
					BaseName = dbName,
					Title = title ?? dbName,
					Version = version
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
						{ "BaseTitle", dbInfo.Title }
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
