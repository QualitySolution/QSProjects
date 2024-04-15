using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver.MySqlConnector;
using QS.Dialog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Services;
using System;
using System.Linq;
using System.Reflection;
using ListenerType = NHibernate.Event.ListenerType;

namespace QS.Project.Core {
	public static class DependencyInjection 
	{
		/// <summary>
		/// Регистрирует основные зависимости Core проекта
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddCore(this IServiceCollection services) {
			services
				.AddMappingAssemblies(Assembly.GetExecutingAssembly())
				.AddSessionFactory()
				.AddSingleton<ISessionProvider, DefaultSessionProvider>()
				.AddSingleton<IOrmConfig, DefaultOrmConfig>()
				.AddUserService()
				;
			return services;
		}

		/// <summary>
		/// Регистрирует зависимости UoW с трекером изменений по умолчанию
		/// В этом случает трекер будет использовать вызываемый поток для регистрации 
		/// событий в БД, что подойдет для приложений без GUI
		/// <para><b>ВАЖНО: Взаимоисключающий с другими методами Add*UoW(services)</b></para>
		/// </summary>
		public static IServiceCollection AddTrackedUoW(this IServiceCollection services) {
			services
				.AddSingleton<ITrackerActionInvoker, DefaultTrackerActionInvoker>()
				.AddSingleton<GlobalUowEventsTracker>()
				.AddTransient<SingleUowEventsTracker>()
				.AddSingleton<IUnitOfWorkFactory, TrackedUnitOfWorkFactory>()
				;
			return services;
		}

		/// <summary>
		/// Регистрирует зависимости UoW без трекера изменений
		/// Изменения в сущностях не будут отслеживаться и не будут сохраняться в истории изменений
		/// <para><b>ВАЖНО: Взаимоисключающий с другими методами Add*UoW(services)</b></para>
		/// </summary>
		public static IServiceCollection AddNotTrackedUoW(this IServiceCollection services) {
			services
				.AddSingleton<IUnitOfWorkFactory>(sp => {
					var uowFactory = new NotTrackedUnitOfWorkFactory(sp.GetRequiredService<ISessionProvider>());
					return uowFactory;
				})
				;

			return services;
		}

		/// <summary>
		/// Регистрирует фабрику сесиий NHibernate. 
		/// Перед этим собирает конфигурацию и производит первоначальное подключение к базе данных
		/// Так же вызывает статические инициализации, которые должны быть вызваны после инициализации базы данных
		/// </summary>
		public static IServiceCollection AddSessionFactory(this IServiceCollection services) {
			services.AddSingleton<ISessionFactory>((provider) => {
				var databaseConfiguration = provider.GetRequiredService<Configuration>();
				var factory = databaseConfiguration.BuildSessionFactory();
				
				//Вызов инициализаций которые пока еще не могут быть добавлены в контейнер правльным образом
				provider.GetServices<OnDatabaseInitialization>();

				return factory;
			});
			return services;
		}

		/// <summary>
		/// Регистрирует настройки подключения к базе данных,
		/// загружая их из секции <see cref="DatabaseConnectionSettings"/> файла конфигурации,
		/// если не передан аргумент <paramref name="settingName"/>
		/// </summary>
		public static IServiceCollection AddDatabaseConnectionSettings(this IServiceCollection services, string settingName = "") {
			var resultSettingName = string.IsNullOrWhiteSpace(settingName) ? nameof(DatabaseConnectionSettings) : settingName;
			services.AddSingleton<IDatabaseConnectionSettings>((provider) => {
				var settings = new DatabaseConnectionSettings();
				var section = provider.GetRequiredService<IConfiguration>().GetSection(resultSettingName);
				section.Bind(settings);
				return settings;
			});

			return services;
		}

		/// <summary>
		/// Регистрирует настроенный билдер строки подключения к базе данных,
		/// данные для подключения берутся из <see cref="IDatabaseConnectionSettings"/>
		/// </summary>
		public static IServiceCollection AddDatabaseConnectionString(this IServiceCollection services) {
			services.AddSingleton<MySqlConnectionStringBuilder>((provider) => {
				var connectionSettings = provider.GetRequiredService<IDatabaseConnectionSettings>();
				var builder = new MySqlConnectionStringBuilder {
					Server = connectionSettings.ServerName,
					Port = connectionSettings.Port,
					Database = connectionSettings.DatabaseName,
					UserID = connectionSettings.UserName,
					Password = connectionSettings.Password,
					SslMode = connectionSettings.MySqlSslMode
				};

				if(connectionSettings.DefaultCommandTimeout.HasValue) {
					builder.DefaultCommandTimeout = connectionSettings.DefaultCommandTimeout.Value;
				}

				return builder;
			});

			return services;
		}

		/// <summary>
		/// Регистрирует настройки SQL для подключения к базе данных
		/// </summary>
		public static IServiceCollection AddSqlConfiguration(this IServiceCollection services) {
			services.AddSingleton<MySQLConfiguration>((provider) => {
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				var dbConfig = MySQLConfiguration.Standard
					.Dialect<MySQL5Dialect>()
					.ConnectionString(connectionStringBuilder.ConnectionString)
					.Driver<MySqlConnectorDriver>()
				;
				return dbConfig;
			});

			return services;
		}

		/// <summary>
		/// Регистрирует конфигурацию NHibernate.
		/// Подключает к конфигурации конвенции, трекеры, сборки с маппингами сущностей
		/// </summary>
		public static IServiceCollection AddNHibernateConfiguration(this IServiceCollection services) {
			services.AddSingleton<Configuration>((provider) => {
				var sqlConfiguration = provider.GetRequiredService<MySQLConfiguration>();
				var assembliesProviders = provider.GetServices<IMappingAssembliesProvider>();
				var configurationExposer = provider.GetService<IDatabaseConfigurationExposer>();
				var conventions = provider.GetServices<IConvention>();

				var fluentConfig = Fluently.Configure().Database(sqlConfiguration);
				fluentConfig.Mappings(m => {
					if(conventions != null && conventions.Any()) {
						m.FluentMappings.Conventions.Add(conventions.ToArray());
					}
					if(assembliesProviders != null) {
						var mappingAssemblies = assembliesProviders.SelectMany(x => x.GetMappingAssemblies()).Distinct();
						foreach(var assembly in mappingAssemblies) {
							m.FluentMappings.AddFromAssembly(assembly);
						}
					}
					
				});

				var tracker = provider.GetService<GlobalUowEventsTracker>();
				if(tracker != null) {
					var listeners = new[] { tracker };
					fluentConfig.ExposeConfiguration(cfg => {
						cfg.AppendListeners(ListenerType.PostLoad, listeners);
						cfg.AppendListeners(ListenerType.PreLoad, listeners);
						cfg.AppendListeners(ListenerType.PostDelete, listeners);
						cfg.AppendListeners(ListenerType.PostUpdate, listeners);
						cfg.AppendListeners(ListenerType.PostInsert, listeners);
					});
				}
				if(configurationExposer != null) {
					fluentConfig.ExposeConfiguration(configurationExposer.ExposeConfiguration);
				}

				return fluentConfig.BuildConfiguration();
			});
			return services;
		}

		/// <summary>
		/// Регистрирует провайдер сборок с маппингами сущностей,
		/// используемый для настройки ORM
		/// </summary>
		public static IServiceCollection AddMappingAssemblies(this IServiceCollection services, params Assembly[] mappingAssemblies) {
			services.AddSingleton<IMappingAssembliesProvider>(new DefaultMappingAssembliesProvider(mappingAssemblies));
			return services;
		}

		/// <summary>
		/// Регистрирует класс для настройки конфигурации NHibernate
		/// до сборки конфигурации
		/// </summary>
		public static IServiceCollection AddDatabaseConfigurationExposer(this IServiceCollection services, Action<Configuration> exposeConfiguration) {
			services.AddSingleton<IDatabaseConfigurationExposer>(new DefaultDatabaseConfigurationExposer(exposeConfiguration));
			return services;
		}

		/// <summary>
		/// Регистрирует провайдер информации о базе данных,
		/// получая информацию из <see cref="MySqlConnectionStringBuilder"/>
		/// </summary>
		public static IServiceCollection AddDatabaseInfo(this IServiceCollection services) {
			services.AddSingleton<IDataBaseInfo>((provider) => {
				var connectionStringBuilder = provider.GetRequiredService<MySqlConnectionStringBuilder>();
				return new DataBaseInfo(connectionStringBuilder.Database);
			});
			return services;
		}

		public static IServiceCollection AddUserService(this IServiceCollection services) {
			services.AddSingleton<IUserService>((provider) => {
				var uowFactory = provider.GetRequiredService<IUnitOfWorkFactory>();
				var connectionSettings = provider.GetRequiredService<IDatabaseConnectionSettings>();
				using(var uow = uowFactory.CreateWithoutRoot()) {
					var serviceUser = uow.Session.Query<UserBase>()
						.Where(u => u.Login == connectionSettings.UserName)
						.FirstOrDefault();

					if(serviceUser is null) {
						throw new InvalidOperationException("Service user not found");
					}

					UserRepository.GetCurrentUserId = () => serviceUser.Id;
					return new UserService(serviceUser);
				}
			});
			return services;
		}
	}
}
