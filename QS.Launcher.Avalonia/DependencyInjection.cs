using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.Services;
using QS.Launcher.Views;

namespace QS.Launcher;
public static partial class DependencyInjection {
	public static IServiceCollection AddPages(this IServiceCollection services) {
		return services
			.AddSingleton<MainWindow>()
			.AddSingleton<IUiThreadInvoker, AvaloniaUiThreadInvoker>();
	}
}
