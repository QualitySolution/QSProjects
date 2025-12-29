using Dapper;
using MySqlConnector;
using NUnit.Framework;
using System.Threading.Tasks;
using Testcontainers.MariaDb;

namespace QS.Testing.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой данных MariaDB в Docker-контейнере.
	/// Предназначен для тестов, работающих напрямую с SQL через Dapper или MySqlConnector.
	/// </summary>
	public abstract class MariaDbTestContainerSqlFixtureBase
	{
		protected MariaDbContainer MariaDbContainer;
		protected string DefaultDbName { get; set; }

		/// <summary>
		/// Конструктор. Автоматически формирует имя базы данных на основе имени тестового класса.
		/// </summary>
		protected MariaDbTestContainerSqlFixtureBase()
		{
			// Автоматически формируем имя базы на основе имени класса
			DefaultDbName = $"{GetType().Name}";
		}

		/// <summary>
		/// Конструктор с возможностью задать имя базы данных вручную
		/// </summary>
		/// <param name="dbName">Имя базы данных</param>
		protected MariaDbTestContainerSqlFixtureBase(string dbName)
		{
			DefaultDbName = dbName;
		}

		/// <summary>
		/// Инициализация контейнера MariaDB. Вызывается в OneTimeSetUp.
		/// </summary>
		[OneTimeSetUp]
		public virtual async Task OneTimeSetUp()
		{
			await InitialiseMariaDb();
		}

		/// <summary>
		/// Остановка и удаление контейнера MariaDB. Вызывается в OneTimeTearDown.
		/// </summary>
		[OneTimeTearDown]
		public virtual async Task OneTimeTearDown()
		{
			await DisposeMariaDb();
		}

		/// <summary>
		/// Инициализация контейнера MariaDB
		/// </summary>
		protected async Task InitialiseMariaDb()
		{
			if (MariaDbContainer != null)
				return;

			// Запуск контейнера MariaDB без создания базы по умолчанию
			MariaDbContainer = new MariaDbBuilder()
				.WithUsername("root")
				.WithPassword("root")
				.WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_general_ci")
				.Build();

			await MariaDbContainer.StartAsync();
		}

		/// <summary>
		/// Очистка и остановка контейнера
		/// </summary>
		protected async Task DisposeMariaDb()
		{
			if (MariaDbContainer != null)
			{
				await MariaDbContainer.DisposeAsync();
				MariaDbContainer = null;
			}
		}

		protected MySqlConnectionStringBuilder GetConnectionStringBuilder(string dbName = null, bool enableUserVariables = false, bool withoutDb = false) {
			var csb = new MySqlConnectionStringBuilder(MariaDbContainer.GetConnectionString());
			
			csb.Database = withoutDb ? null : (dbName ?? DefaultDbName);

			if (enableUserVariables)
				csb.AllowUserVariables = true;
			
			return csb;
		}
		
		/// <summary>
		/// Получить строку подключения к базе данных
		/// </summary>
		/// <param name="dbName">Имя базы данных. Если null - соединение на уровне сервера (без выбранной БД)</param>
		/// <param name="enableUserVariables">Включить поддержку пользовательских переменных</param>
		/// <param name="withoutDb">Если true - подключение без указания базы данных</param>
		/// <returns>Строка подключения</returns>
		protected string GetConnectionString(string dbName = null, bool enableUserVariables = false, bool withoutDb = false) {
			return GetConnectionStringBuilder(dbName, enableUserVariables, withoutDb).ConnectionString;
		}

		/// <summary>
		/// Создать новое подключение к базе данных
		/// </summary>
		/// <param name="dbName">Имя базы данных. Если null - соединение на уровне сервера (без выбранной БД)</param>
		/// <param name="enableUserVariables">Включить поддержку пользовательских переменных</param>
		/// <param name="withoutDb">Если true - подключение без указания базы данных</param>
		/// <returns>Новое подключение MySqlConnection</returns>
		protected MySqlConnection CreateConnection(string dbName = null, bool enableUserVariables = false, bool withoutDb = false) {
			return new MySqlConnection(GetConnectionString(dbName, enableUserVariables, withoutDb));
		}

		/// <summary>
		/// Пересоздать базу данных (удалить и создать заново)
		/// </summary>
		protected async Task RecreateDatabase(string dbName = null)
		{
			if (dbName == null)
				dbName = DefaultDbName;

			using (var connection = CreateConnection(withoutDb: true))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{dbName}`;");
				await connection.ExecuteAsync($"CREATE DATABASE `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");
			}
		}

		/// <summary>
		/// Выполнить SQL скрипт создания схемы базы данных
		/// </summary>
		/// <param name="connection">Рабочее соединение с базой</param>
		/// <param name="createSchemaScript">SQL скрипт создания таблиц</param>
		/// <param name="dbName">Имя подготавливаемой базы, если не указана создается база по умолчанию <see cref="DefaultDbName"/> </param>
		protected async Task PrepareDatabase(string createSchemaScript, MySqlConnection connection = null, string dbName = null) {
			if(dbName == null)
				dbName = DefaultDbName;
			
			await RecreateDatabase(dbName);
			bool needToDisposeConnection = false;
			
			if(connection == null) {
				connection = CreateConnection(dbName, true);
				await connection.OpenAsync();
				needToDisposeConnection = true;
			}
			
			await connection.ExecuteAsync($"USE `{dbName}`;");
			await connection.ExecuteAsync(createSchemaScript, commandTimeout: 180);
			
			if(needToDisposeConnection) {
				await connection.CloseAsync();
			}
		}
	}
}
