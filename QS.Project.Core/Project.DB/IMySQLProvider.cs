using System;
using MySqlConnector;

namespace QS.Project.DB
{
	public interface IMySQLProvider
	{
		MySqlConnection DbConnection { get; }
		void CheckConnectionAlive();
		void TryConnect();
	}
}
