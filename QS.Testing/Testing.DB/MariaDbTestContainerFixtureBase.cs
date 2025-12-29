using System.Reflection;
using System.Threading.Tasks;
using FluentNHibernate.Cfg.Db;
using MySqlConnector;
using NHibernate.Cfg;
using NHibernate.Driver.MySqlConnector;
using NHibernate.Tool.hbm2ddl;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Testcontainers.MariaDb;

namespace QS.Testing.DB
{
	/// <summary>
	/// Базовый класс для тестирования работы с базой данных MariaDB в Docker-контейнере.
	/// Схема базы данных создается через Hibernate на основе маппингов.
	/// </summary>
	public abstract class MariaDbTestContainerFixtureBase
	{
		protected MariaDbContainer MariaDbContainer;
		protected Configuration Configuration;
		protected IUnitOfWorkFactory UnitOfWorkFactory;
		protected string ConnectionString;
		protected MySqlConnectionStringBuilder ConnectionStringBuilder;
		private const string DbName = "test_db";

		/// <summary>
		/// Инициализация контейнера MariaDB и конфигурации NHibernate
		/// </summary>
		protected async Task InitialiseMariaDb(params Assembly[] assemblies)
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

			// Полный сброс глобального состояния OrmConfig для изоляции от предыдущих тестов
			OrmConfig.ResetForTesting();
			
			// Конфигурация NHibernate для работы с MariaDB
			var dbConfig = MySQLConfiguration.Standard
				.Driver<MySqlConnectorDriver>()
				.ConnectionString(ConnectionString)
				.ShowSql();

			OrmConfig.ConfigureOrm(dbConfig, assemblies);
			Configuration = OrmConfig.NhConfig;

			// Создание схемы базы данных через Hibernate
			await CreateDatabaseSchema();

			// Создание фабрики UnitOfWork
			UnitOfWorkFactory = new DefaultUnitOfWorkFactory(new DefaultSessionProvider());
		}

		/// <summary>
		/// Создание схемы базы данных с помощью SchemaExport
		/// </summary>
		private async Task CreateDatabaseSchema()
		{
			using(var connection = new MySqlConnection(ConnectionString)) {
				await connection.OpenAsync();
				
				var schemaExport = new SchemaExport(Configuration);
				schemaExport.Execute(false, true, false, connection, null);
			}
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
		/// Пересоздание схемы базы данных (для очистки между тестами)
		/// </summary>
		protected async Task RecreateSchema()
		{
			using(var connection = new MySqlConnection(ConnectionString)) {
				await connection.OpenAsync();

				var schemaExport = new SchemaExport(Configuration);
				// Удаляем старую схему
				schemaExport.Execute(false, true, true, connection, null);
				// Создаем новую
				schemaExport.Execute(false, true, false, connection, null);
			}
		}

		/// <summary>
		/// Получить соединение с базой данных для прямых SQL-запросов
		/// </summary>
		protected MySqlConnection GetConnection()
		{
			return new MySqlConnection(ConnectionString);
		}
	}
}
