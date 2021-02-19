using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace QS.Project.DB
{
	public class MySqlConnectionFactory : IConnectionFactory
	{
		private readonly string connectionString;

		public MySqlConnectionFactory(string ConnectionString)
		{
			connectionString = ConnectionString ?? throw new ArgumentNullException(nameof(ConnectionString));
		}

		public MySqlConnection OpenMySqlConnection()
		{
			var connection = new MySqlConnection(connectionString);
			connection.Open();
			return connection;
		}

		public DbConnection OpenConnection()
		{
			return OpenMySqlConnection();
		}
	}
}
