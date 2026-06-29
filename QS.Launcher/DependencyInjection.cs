using Autofac;
using Microsoft.Extensions.DependencyInjection;
using QS.DBScripts.Controllers;
using QS.DbManagement;
using QS.Launcher.AppRunner;
using QS.Launcher.Services;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.DbManagement.Creation;
using System;
using System.Collections.Generic;

namespace QS.Launcher {
	public static partial class DependencyInjection {
		public static IServiceCollection AddLauncherDataBaseCreation(this IServiceCollection services, List<(Type res,Type creator)> resourceCratorMap)
		{
			var map = new DbResourcesCreationMap();
			foreach(var resourceCrator in resourceCratorMap) {
				map.Register(resourceCrator.res, resourceCrator.creator);
			}

			return services
				.AddSingleton(map)
				.AddSingleton<DbCreationFactory>();
		}

		public static IServiceCollection AddLauncherViewModels(this IServiceCollection services) {
			return services
				.AddSingleton<MainWindowVM>()
				.AddSingleton<LoginVM>()
				.AddSingleton<DataBasesVM>()
				.AddSingleton<UserManagementVM>()
				// Страница прогресса создаётся заново на каждую операцию с базой
				.AddTransient<CreateDataBaseProgressVM>()
				.AddSingleton<IDbCreatorInteraction, LauncherDbCreatorInteraction>()
				.AddSingleton<DbCapabilities>();
		}

		public static IServiceCollection AddLauncherOptions(this IServiceCollection services, LauncherOptions launcherOptions) {
			return services.AddSingleton(launcherOptions);
		}
		
		public static IServiceCollection AddLauncherDependencies(this IServiceCollection services) {
			return services
				.AddSingleton<Configurator>();
		} 

		public static IServiceCollection AddConnectionType(this IServiceCollection services, ConnectionTypeBase connectionType) {
			services.AddSingleton(connectionType);
			return services;
		}

		#region AppRunner

		public static IServiceCollection UseInProcessRunner(this IServiceCollection services) {
			services.AddSingleton<InProcessRunner>();
			services.AddSingleton<IAppRunner>(sp => sp.GetRequiredService<InProcessRunner>());
			return services;
		}
		
		// Для Autofac - регистрируем как интерфейс и класс одновременно
		public static void UseInProcessRunner(this ContainerBuilder builder) {
			builder.RegisterType<InProcessRunner>().As<IAppRunner>().AsSelf().SingleInstance();
		}
		
		public static IServiceCollection UseNewProcessRunner(this IServiceCollection services, string executableFileName) {
			return services.AddSingleton<IAppRunner>(c => new NewProcessRunner(executableFileName));
		}

		#endregion
	}
}
