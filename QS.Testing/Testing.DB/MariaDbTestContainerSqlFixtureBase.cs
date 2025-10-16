using Dapper;
using MySqlConnector;
using NUnit.Framework;
using System.Data;
using System.Threading.Tasks;
using System;
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
		protected string ConnectionString;
		protected MySqlConnectionStringBuilder ConnectionStringBuilder;
		protected string DbName { get; set; }

		/// <summary>
		/// Конструктор. Автоматически формирует имя базы данных на основе имени тестового класса.
		/// </summary>
		protected MariaDbTestContainerSqlFixtureBase()
		{
			// Автоматически формируем имя базы на основе имени класса
			DbName = $"{GetType().Name}";
		}

		/// <summary>
		/// Конструктор с возможностью задать имя базы данных вручную
		/// </summary>
		/// <param name="dbName">Имя базы данных</param>
		protected MariaDbTestContainerSqlFixtureBase(string dbName)
		{
			DbName = dbName;
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

			// Запуск контейнера MariaDB
			MariaDbContainer = new MariaDbBuilder()
				.WithDatabase(DbName)
				.WithCommand("--character-set-server=utf8mb4", "--collation-server=utf8mb4_general_ci")
				.Build();

			await MariaDbContainer.StartAsync();
			
			// Формирование строки подключения
			ConnectionString = MariaDbContainer.GetConnectionString() + ";Allow User Variables=true;CharSet=utf8mb4";
			ConnectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionString);
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

		/// <summary>
		/// Создать новое подключение к базе данных
		/// </summary>
		/// <param name="enableUserVariables">Включить поддержку пользовательских переменных</param>
		/// <returns>Новое подключение MySqlConnection</returns>
		protected MySqlConnection CreateConnection(bool enableUserVariables = false)
		{
			var connectionString = ConnectionString;
			if (enableUserVariables && !connectionString.Contains("Allow User Variables"))
			{
				connectionString += ";Allow User Variables=true";
			}
			return new MySqlConnection(connectionString);
		}

		/// <summary>
		/// Пересоздать базу данных (удалить и создать заново)
		/// </summary>
		protected async Task RecreateDatabase()
		{
			// Выполняем операции на уровне сервера без выбранной БД, иначе ловим эксепшен что такой базы нет существует. Что как будтобы логично.
			var csb = new MySqlConnectionStringBuilder(MariaDbContainer.GetConnectionString()) { Database = string.Empty };
			using (var connection = new MySqlConnection(csb.ConnectionString))
			{
				await connection.OpenAsync();
				await connection.ExecuteAsync($"DROP DATABASE IF EXISTS `{DbName}`;");
				await connection.ExecuteAsync($"CREATE DATABASE `{DbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;");
			}
		}

		/// <summary>
		/// Выполнить SQL скрипт создания схемы базы данных
		/// </summary>
		/// <param name="createSchemaScript">SQL скрипт создания таблиц</param>
		protected async Task PrepareDatabase(string createSchemaScript) {
			using(var connection = CreateConnection(enableUserVariables: true)) {
				await connection.OpenAsync();
				await PrepareDatabase(connection, createSchemaScript);
			}
		}

		/// <summary>
		/// Выполнить SQL скрипт создания схемы базы данных
		/// </summary>
		/// <param name="connection">Рабочее соединение с базой</param>
		/// <param name="createSchemaScript">SQL скрипт создания таблиц</param>
		protected async Task PrepareDatabase(MySqlConnection connection, string createSchemaScript) {
			await RecreateDatabase();

			// Открываем переданное соединение к только что созданной базе и применяем схему
			if(connection.State != ConnectionState.Open)
				await connection.OpenAsync();
			await connection.ExecuteAsync($"USE `{DbName}`;");
			await connection.ExecuteAsync(createSchemaScript, commandTimeout: 180);
		}
	}
}
