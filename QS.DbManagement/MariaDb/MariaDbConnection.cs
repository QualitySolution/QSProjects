using Dapper;
using MySqlConnector;
using System;

namespace QS.DbManagement
{
	public class MariaDbConnection : IDbProvider
	{
		readonly MySqlConnection connection;
	
		public string ConnectionString
		{
			get => connection.ConnectionString;
			set => connection.ConnectionString = value;
		}
	
		public bool IsConnected => throw new NotImplementedException();
	
		public MariaDbConnection(string connectionString)
		{
			ConnectionString = connectionString;
			connection = new MySqlConnection(connectionString);
		}
	
		public bool AddUser(string username, string password)
		{
			string sql = $"CREATE USER IF NOT EXISTS '{username}' IDENTIFIED BY '{password}'";
	
			return connection.Execute(sql) != 0;
		}
	
		public bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			string sql = $"ALTER USER '{username}'@'%' IDENTIFIED BY '{newPassword}'";
	
			return connection.Execute(sql) != 0;
		}
	
		public bool CreateDatabase(string databaseName)
		{
			string sql = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
	
			return connection.Execute(sql) != 0;
		}
	
		public void Dispose()
		{
			connection.Dispose();
		}
	
		public bool DropDatabase(string databaseName)
		{
			string sql = $"DROP DATABASE IF EXISTS `{databaseName}`";
	
			return connection.Execute(sql) != 0;
		}
	}

}

