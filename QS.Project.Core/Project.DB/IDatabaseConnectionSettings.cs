using MySqlConnector;

namespace QS.Project.DB {
	public interface IDatabaseConnectionSettings {
		string ServerName { get; set; }
		uint Port { get; set; }
		string DatabaseName { get; set; }
		string UserName { get; set; }
		string Password { get; set; }
		MySqlSslMode MySqlSslMode { get; set; }
		uint? DefaultCommandTimeout { get; set; }
	}
}
