using Dapper;
using MySqlConnector;
using QS.DbManagement.Responces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DbManagement
{
	public class MariaDBProvider : IDbProvider {
		readonly MySqlConnection connection;
	
		public string ConnectionString
		{
			get => connection.ConnectionString;
			set => connection.ConnectionString = value;
		}
	
		public bool IsConnected => throw new NotImplementedException();

		public ConnectionInfo ConnectionInfo => throw new NotImplementedException();

		public bool IsAdmin => throw new NotImplementedException();

		public MariaDBProvider(MariaDBConnectionInfo connectionInfo)
		{
			ConnectionString = connectionInfo.Parameters.First(p => p.Title == "ConnectionString").Value as string;
			connection = new MySqlConnection(ConnectionString);
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

		public List<DbInfo> GetUserDatabases() {
			throw new NotImplementedException();
		}

		public LoginToDatabaseResponce LoginToDatabase(DbInfo dbInfo) {
			throw new NotImplementedException();
		}

		LoginToServerResponce IDbProvider.LoginToServer(LoginToServerData loginToServerData) {
			throw new NotImplementedException();
		}
	}

}

