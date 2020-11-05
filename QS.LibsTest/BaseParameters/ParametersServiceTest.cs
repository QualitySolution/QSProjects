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

		[Test(Description = "Проверка что успешно читаем строковое значение из базы.")]
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
		#region SetValues
		[Test(Description = "Проверка что можем создать параметр.")]
		public void SetValue_NewParameter_StringCase()
		{
			using (var connection = new SqliteConnection(connectionString)) {
				MakeTable(connection);
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = "String result";
				Assert.That(parameters.StringParameter, Is.EqualTo("String result"));
				var inDB = connection.ExecuteScalar<string>(sqlSelect, new { name = "StringParameter" });
				Assert.That(inDB, Is.EqualTo("String result"));
			}
		}

		[Test(Description = "Проверка что можем обновить значение параметра.")]
		public void SetValue_UpdateParameter_StringCase()
		{
			using (var connection = new SqliteConnection(connectionString)) {
				MakeTable(connection);
				connection.Execute(sqlInsert, new { name = "StringParameter", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = "String2";
				Assert.That(parameters.StringParameter, Is.EqualTo("String2"));
				var inDB = connection.ExecuteScalar<string>(sqlSelect, new { name = "StringParameter" });
				Assert.That(inDB, Is.EqualTo("String2"));
			}
		}

		[Test(Description = "Проверка что можем удалить параметр установил значение в null.")]
		public void SetValue_RemoveParameter_StringCase()
		{
			using (var connection = new SqliteConnection(connectionString)) {
				MakeTable(connection);
				connection.Execute(sqlInsert, new { name = "StringParameter", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = null;
				Assert.That(parameters.StringParameter, Is.Null);
				var rows = connection.Execute(sqlSelect, new { name = "StringParameter" });
				Assert.That(rows, Is.EqualTo(0));
			}
		}

		#endregion
		#region Утилиты
		const string connectionString = "Data Source=:memory:;Version=3;New=True;";
		const string sqlInsert = "INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);";
		const string sqlSelect = "SELECT str_value FROM base_parameters WHERE name = @name;";

		void MakeTable(SqliteConnection connection)
		{
			connection.Open();
			connection.Execute("CREATE TABLE `base_parameters` (`name` varchar(20) NOT NULL, `str_value` varchar(100) DEFAULT NULL);");
		}
		#endregion
	}
}
