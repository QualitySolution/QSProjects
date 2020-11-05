using Dapper;
using Mono.Data.Sqlite;
using NUnit.Framework;
using QS.BaseParameters;

namespace QS.Test.BaseParameters
{
	[TestFixture(TestOf = typeof(ParametersService))]
	public class ParametersServiceTest
	{
		#region GetValueTests
		[Test(Description = "Проверка что не падаем, а возвращаем null если указанного параметра в базе нет.")]
		public void GetValue_ReturnNullCase()
		{
			using (var connection = new SqliteConnection(connectionString)) {
				MakeTable(connection);
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.NotExist, Is.Null);
			}
		}

		[Test(Description = "Проверка что не падаем, а возвращаем null если указанного параметра в базе нет.")]
		public void GetValue_ReturnStringCase()
		{
			using (var connection = new SqliteConnection(connectionString)) {
				MakeTable(connection);
				connection.Execute(sqlInsert, new { name = "GetStringValueTest", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.GetStringValueTest, Is.EqualTo("String result"));
			}
		}
		#endregion

		#region Утилиты
		const string connectionString = "Data Source=:memory:;Version=3;New=True;";
		const string sqlInsert = "INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);";

		void MakeTable(SqliteConnection connection)
		{
			connection.Open();
			connection.Execute("CREATE TABLE `base_parameters` (`name` varchar(20) NOT NULL, `str_value` varchar(100) DEFAULT NULL);");
		}
		#endregion
	}
}
