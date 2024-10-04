using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;
using System.Collections.Generic;

namespace QS.Launcher {
	public static partial class DependencyInjection {
		public static IServiceCollection AddViewModels(this IServiceCollection services) {
			return services
				.AddSingleton<MainWindowVM>()
				.AddSingleton<LoginVM>()
				.AddSingleton<DataBasesVM>()
				.AddSingleton<UserManagementVM>()
				.AddSingleton<BaseManagementVM>();
		}

		public static IServiceCollection AddCompanyDependencies(this IServiceCollection services, LauncherOptions launcherOptions) {
			return services.AddSingleton(launcherOptions);
		}

		public static IServiceCollection AddConnectionTypes(this IServiceCollection services, IEnumerable<ConnectionInfo> connectionInfos) {
			foreach(var connectionInfo in connectionInfos)
				services.AddSingleton(connectionInfo);
			return services;
		}
	}
}
