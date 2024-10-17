using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.Views;

namespace QS.Launcher {
	public static partial class DependencyInjection {
		public static IServiceCollection AddLauncherViews(this IServiceCollection services) {
			return services
				.AddSingleton<MainWindow>()
				.AddSingleton<LoginView>()
				.AddSingleton<DataBasesView>();
		}
	}
}
