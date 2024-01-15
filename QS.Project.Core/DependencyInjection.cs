using FluentNHibernate.Cfg;
using FluentNHibernate.Conventions;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using QS.Dialog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System.Linq;

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
				.AddDatabaseConfiguration()
				.AddSessionFactory()
				.AddSingleton<ISessionProvider, DefaultSessionProvider>()
				.AddSingleton<IOrmConfig, DefaultOrmConfig>()
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
				.AddSingleton<IUnitOfWorkFactory, NotTrackedUnitOfWorkFactory>()
				;

			return services;
		}

		public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services) {
			services.AddSingleton<Configuration>((provider) => {
				var ormSettings = provider.GetRequiredService<IOrmSettings>();
				var fluentConfig = Fluently.Configure().Database(ormSettings.GetDatabaseConfiguration(provider));
				var conventions = provider.GetServices<IConvention>();

				fluentConfig.Mappings(m => {
					if(conventions != null && conventions.Any()) {
						m.FluentMappings.Conventions.Add(conventions.ToArray());
					}
					foreach(var ass in ormSettings.GetMappingAssemblies()) {
						m.FluentMappings.AddFromAssembly(ass);
					}
				});

				var tracker = provider.GetService<GlobalUowEventsTracker>();
				if(tracker != null) {
					var listeners = new[] { tracker };
					fluentConfig.ExposeConfiguration(cfg => {
						cfg.AppendListeners(NHibernate.Event.ListenerType.PostLoad, listeners);
						cfg.AppendListeners(NHibernate.Event.ListenerType.PreLoad, listeners);
						cfg.AppendListeners(NHibernate.Event.ListenerType.PostDelete, listeners);
						cfg.AppendListeners(NHibernate.Event.ListenerType.PostUpdate, listeners);
						cfg.AppendListeners(NHibernate.Event.ListenerType.PostInsert, listeners);
					});
				}

				fluentConfig.ExposeConfiguration(ormSettings.ExposeConfiguration);

				return fluentConfig.BuildConfiguration();
			});
			return services;
		}

		public static IServiceCollection AddSessionFactory(this IServiceCollection services) {
			services.AddSingleton<ISessionFactory>((provider) => {
				var databaseConfiguration = provider.GetRequiredService<Configuration>();
				return databaseConfiguration.BuildSessionFactory();
			});
			return services;
		}
	}
}
