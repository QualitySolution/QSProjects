using System;
using System.Data.Common;
using MySqlConnector;

namespace QS.Project.DB
{
	public class MySqlConnectionFactory : IConnectionFactory
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly string connectionString;

		public MySqlConnectionFactory(string сonnectionString)
		{
			connectionString = сonnectionString ?? throw new ArgumentNullException(nameof(сonnectionString));
		}

		public MySqlConnection OpenMySqlConnection()
		{
			var connection = new MySqlConnection(connectionString);
			connection.Open();
			logger.Debug("Открыто новое соединение с БД");
			connection.StateChange += (sender, e) => logger.Debug($"Cоединение с БД теперь: {e.CurrentState}");
			return connection;
		}

		public DbConnection OpenConnection()
		{
			return OpenMySqlConnection();
		}
	}
}
