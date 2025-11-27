using System;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Testing.DB;

namespace QS.Test.BaseParameters
{
	[TestFixture(TestOf = typeof(ParametersService))]
	public class ParametersServiceTest : MariaDbTestContainerSqlFixtureBase
	{
		[OneTimeSetUp]
		public async Task GlobalSetup()
		{
			await InitialiseMariaDb();
			await PrepareDatabase();
		}

		private async Task PrepareDatabase()
		{
			await RecreateDatabase(DefaultDbName);
			using (var connection = CreateConnection())
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync(@"
					CREATE TABLE `base_parameters` (
						`name` varchar(20) NOT NULL,
						`str_value` varchar(100) DEFAULT NULL,
						PRIMARY KEY (`name`)
					) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
				");
			}
		}

		[SetUp]
		public async Task SetUp()
		{
			// Очистка таблицы перед каждым тестом
			using (var connection = CreateConnection())
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync("DELETE FROM base_parameters;");
			}
		}
		#region GetValueTests
		[Test(Description = "Проверка что не падаем, а возвращаем null если указанного параметра в базе нет.")]
		public async Task GetValue_ReturnNullCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.NotExist, Is.Null);
			}
		}

		[Test(Description = "Проверка что успешно читаем строковое значение из базы.")]
		public async Task GetValue_ReturnStringCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "GetStringValueTest", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.GetStringValueTest, Is.EqualTo("String result"));
			}
		}

		#region Цифры

		[Test(Description = "Проверка что успешно читаем значение int.")]
		public async Task GetValue_ReturnIntCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "Number", str_value = "42" });
				dynamic parameters = new ParametersService(connection);
				int value = parameters.Number(typeof(int));
				Assert.That(value, Is.EqualTo(42));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение long.")]
		public async Task GetValue_ReturnLongCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "Number", str_value = "9223372036854775807" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.Number(typeof(long)), Is.EqualTo(9223372036854775807));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение uint.")]
		public async Task GetValue_ReturnUIntCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "Number", str_value = "2" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.Number(typeof(uint)), Is.EqualTo(2));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение double.")]
		public async Task GetValue_ReturnDoubleCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "PI", str_value = "3.14159265359" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.PI(typeof(double)), Is.EqualTo(3.14159265359d));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение Decimal.")]
		public async Task GetValue_ReturnDecimalCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "USD", str_value = "76.76" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.USD(typeof(decimal)), Is.EqualTo(76.76m));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение int?.")]
		public async Task GetValue_ReturnIntNulableCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "Number", str_value = 42 });
				dynamic parameters = new ParametersService(connection);
				int? value = parameters.NotExist(typeof(int?));
				Assert.That(value, Is.Null);
				value = parameters.Number(typeof(int?));
				Assert.That(value, Is.EqualTo(42));
			}
		}

		#endregion

		[Test(Description = "Проверка что успешно читаем значение Version.")]
		public async Task GetValue_ReturnVersionCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "version", str_value = "2.3.4" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.version(typeof(Version)), Is.EqualTo(new Version(2, 3, 4)));
			}
		}

		[Test(Description = "Проверка что успешно читаем значение bool.")]
		public async Task GetValue_ReturnBooleanCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", 
					new { name = "IsTrue", str_value = "True" });
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.IsTrue(typeof(bool)), Is.EqualTo(true));
			}
		}

		#endregion
		#region SetValues
		[Test(Description = "Проверка что можем создать параметр.")]
		public async Task SetValue_NewParameter_StringCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = "String result";
				Assert.That(parameters.StringParameter, Is.EqualTo("String result"));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "StringParameter" });
				Assert.That(inDB, Is.EqualTo("String result"));
			}
		}

		#region Другие типы
		[Test(Description = "Проверка что можем создать параметр из int.")]
		public async Task SetValue_NewParameter_IntCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				parameters.Answer = 42;
				Assert.That(parameters.Answer(typeof(int)), Is.EqualTo(42));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "Answer" });
				Assert.That(inDB, Is.EqualTo("42"));
			}
		}

		[Test(Description = "Проверка что можем создать параметр из decimal.")]
		public async Task SetValue_NewParameter_DecimalCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				parameters.VodkaPrice = 2.87m;
				Assert.That(parameters.VodkaPrice(typeof(decimal)), Is.EqualTo(2.87m));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "VodkaPrice" });
				Assert.That(inDB, Is.EqualTo("2.87"));
			}
		}

		[Test(Description = "Проверка что можем создать параметр из bool.")]
		public async Task SetValue_NewParameter_BooleanCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				parameters.IsTest = true;
				Assert.That(parameters.IsTest(typeof(bool)), Is.EqualTo(true));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "IsTest" });
				Assert.That(inDB, Is.EqualTo("True"));
			}
		}

		[Test(Description = "Проверка что можем создать параметр из Version.")]
		public async Task SetValue_NewParameter_VersionCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				parameters.Version = new Version(2, 4);
				Assert.That(parameters.Version(typeof(Version)), Is.EqualTo(new Version(2, 4)));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "Version" });
				Assert.That(inDB, Is.EqualTo("2.4"));
			}
		}

		#endregion

		[Test(Description = "Проверка что можем обновить значение параметра.")]
		public async Task SetValue_UpdateParameter_StringCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", new { name = "StringParameter", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = "String2";
				Assert.That(parameters.StringParameter, Is.EqualTo("String2"));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "StringParameter" });
				Assert.That(inDB, Is.EqualTo("String2"));
			}
		}
		
		[Test(Description = "Проверка что можем обновить значение параметра, если его не было в момент загрузки но появился потом.")]
		public async Task SetValue_UpdateParameter_ExistCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				dynamic parameters = new ParametersService(connection);
				Assert.That(parameters.StringParameter, Is.Null);
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", new { name = "StringParameter", str_value = "String result" });
				parameters.StringParameter = "String2";
				Assert.That(parameters.StringParameter, Is.EqualTo("String2"));
				var inDB = await connection.ExecuteScalarAsync<string>("SELECT str_value FROM base_parameters WHERE name = @name;", new { name = "StringParameter" });
				Assert.That(inDB, Is.EqualTo("String2"));
			}
		}

		[Test(Description = "Проверка что можем удалить параметр установив значение в null.")]
		public async Task SetValue_RemoveParameter_StringCase()
		{
			using (var connection = CreateConnection()) {
				await connection.OpenAsync();
				await connection.ExecuteAsync("INSERT INTO base_parameters (name, str_value) VALUES(@name, @str_value);", new { name = "StringParameter", str_value = "String result" });
				dynamic parameters = new ParametersService(connection);
				parameters.StringParameter = null;
				Assert.That(parameters.StringParameter, Is.Null);
				var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM base_parameters WHERE name = @name;", new { name = "StringParameter" });
				Assert.That(count, Is.EqualTo(0));
			}
		}

		#endregion
	}
}
