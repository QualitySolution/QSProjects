using MySqlConnector;

namespace QS.Project.DB {
	public class DatabaseConnectionSettings : IDatabaseConnectionSettings {
		public string ServerName { get; set; }
		public uint Port { get; set; }
		public string DatabaseName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public MySqlSslMode MySqlSslMode { get; set; }
		public uint? DefaultCommandTimeout { get; set; }

		public DatabaseConnectionSettings(MySqlConnectionStringBuilder stringBuilder) {
			ServerName = stringBuilder.Server;
			Port = stringBuilder.Port;
			DatabaseName = stringBuilder.Database;
			UserName = stringBuilder.UserID;
			Password = stringBuilder.Password;
			MySqlSslMode = stringBuilder.SslMode;
			DefaultCommandTimeout = stringBuilder.ConnectionTimeout;
		}
	}
}
