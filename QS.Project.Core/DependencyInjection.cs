using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Project.DB;

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
				.AddSingleton<IOrmConfig, DefaultOrmConfig>()
				.AddSingleton<ISessionFactory>((ctx) => ctx.GetRequiredService<IOrmConfig>().SessionFactory)
				.AddSingleton<ISessionProvider, ConfiguredSessionFactorySessionProvider>()
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
	}
}
