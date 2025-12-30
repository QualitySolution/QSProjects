using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MySqlConnector;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Driver.MySqlConnector;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using QS.DomainModel.Tracking;
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
		protected ISessionFactory SessionFactory;
		protected ISessionProvider SessionProvider;
		protected IUnitOfWorkFactory UnitOfWorkFactory;
		protected IOrmConfig OrmConfig;
		protected string ConnectionString;
		protected MySqlConnectionStringBuilder ConnectionStringBuilder;
		private const string DbName = "test_db";
		private GlobalUowEventsTracker globalUowEventsTracker;
		private bool useTrackingValue;

		/// <summary>
		/// Использовать ли отслеживание изменений (TrackedUnitOfWorkFactory).
		/// Необходимо для тестов, работающих с HistoryLog или другими механизмами отслеживания изменений.
		/// По умолчанию false для большинства тестов.
		/// ВАЖНО: Должно быть установлено ДО вызова InitialiseMariaDb().
		/// </summary>
		protected bool UseTracking {
			get => useTrackingValue;
			set {
				if(UnitOfWorkFactory != null) {
					throw new System.InvalidOperationException(
						"Свойство UseTracking должно быть установлено ДО вызова InitialiseMariaDb(). " +
						"Изменение режима отслеживания после инициализации не поддерживается.");
				}
				useTrackingValue = value;
			}
		}

		/// <summary>
		/// Инициализация контейнера MariaDB и конфигурации NHibernate
		/// </summary>
		/// <param name="assemblies">Сборки с маппингами NHibernate</param>
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

			// Конфигурация NHibernate для работы с MariaDB через FluentNHibernate
			var dbConfig = MySQLConfiguration.Standard
				.Driver<MySqlConnectorDriver>()
				.ConnectionString(ConnectionString)
				.ShowSql();

			var fluentConfig = Fluently.Configure()
				.Database(dbConfig)
				.Mappings(m => {
					foreach(var assembly in assemblies) {
						m.FluentMappings.AddFromAssembly(assembly);
					}
				});

			// Если требуется отслеживание, регистрируем GlobalUowEventsTracker как NHibernate event listeners
			if(UseTracking) {
				globalUowEventsTracker = new GlobalUowEventsTracker();
				var listeners = new[] { globalUowEventsTracker };
				fluentConfig.ExposeConfiguration(cfg => {
					cfg.AppendListeners(ListenerType.PostLoad, listeners);
					cfg.AppendListeners(ListenerType.PreLoad, listeners);
					cfg.AppendListeners(ListenerType.PostDelete, listeners);
					cfg.AppendListeners(ListenerType.PostUpdate, listeners);
					cfg.AppendListeners(ListenerType.PostInsert, listeners);
				});
			}

			Configuration = fluentConfig.BuildConfiguration();
			SessionFactory = Configuration.BuildSessionFactory();
			SessionProvider = new DefaultSessionProvider(SessionFactory);
			OrmConfig = new DefaultOrmConfig(SessionFactory, Configuration);

			// Создание схемы базы данных через Hibernate
			await CreateDatabaseSchema();

			// Создание фабрики UnitOfWork с поддержкой отслеживания или без
			if(UseTracking) {
				// Настройка Autofac контейнера для TrackedUnitOfWorkFactory
				var builder = new ContainerBuilder();
				builder.RegisterInstance(SessionProvider).As<ISessionProvider>();
				builder.RegisterInstance(OrmConfig).As<IOrmConfig>();
				builder.RegisterType<SingleUowEventsTracker>().AsSelf();
				var container = builder.Build();
				
				// Устанавливаем Scope для устаревшего статического OrmConfig
				// Это необходимо для совместимости с HistoryLog и другим старым кодом
				QS.Project.DB.OrmConfig.Scope = container;
				
				UnitOfWorkFactory = new TrackedUnitOfWorkFactory(container);
			}
			else {
				// Для тестов без отслеживания тоже создаём контейнер для OrmConfig
				var builder = new ContainerBuilder();
				builder.RegisterInstance(SessionProvider).As<ISessionProvider>();
				builder.RegisterInstance(OrmConfig).As<IOrmConfig>();
				var container = builder.Build();
				
				// Устанавливаем Scope для устаревшего статического OrmConfig
				QS.Project.DB.OrmConfig.Scope = container;
				
				// Легковесная фабрика без отслеживания для обычных тестов
				UnitOfWorkFactory = new NotTrackedUnitOfWorkFactory(SessionProvider);
			}
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
