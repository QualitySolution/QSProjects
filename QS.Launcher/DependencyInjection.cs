using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;
using System.Collections.Generic;

namespace QS.Launcher {
	public static partial class DependencyInjection {
		public static IServiceCollection AddLauncherViewModels(this IServiceCollection services) {
			return services
				.AddSingleton<MainWindowVM>()
				.AddSingleton<LoginVM>()
				.AddSingleton<DataBasesVM>()
				.AddSingleton<UserManagementVM>()
				.AddSingleton<BaseManagementVM>();
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
	}
}
