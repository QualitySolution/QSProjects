using Dapper;
using MySqlConnector;
using QS.DbManagement.Responces;
using System.Collections.Generic;
using System;
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

		public bool IsAdmin => throw new NotImplementedException();

		#region Параметры подключени
		public string Server { get; private set; }
		public string UserName { get; private set; }
		#endregion

		public MariaDBProvider(IList<ConnectionParameterValue> parameters, string password = null)
		{
			Server = parameters.First(p => p.Name == "Server").Value;
			UserName = parameters.First(p => p.Name == "Login").Value;
			throw new NotImplementedException();
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

		public LoginToDatabaseResponse LoginToDatabase(DbInfo dbInfo) {
			throw new NotImplementedException();
		}

		LoginToServerResponse IDbProvider.LoginToServer() {
			throw new NotImplementedException();
		}
	}

}

