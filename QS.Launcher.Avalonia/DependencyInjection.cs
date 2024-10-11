using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;

namespace QS.Launcher;
public static partial class DependencyInjection {
	public static IServiceCollection AddPages(this IServiceCollection services) {
		return services
			.AddSingleton<MainWindow>()
			.AddSingleton<UserControl, LoginView>()
			.AddSingleton<UserControl, DataBasesView>()
			.AddSingleton<UserControl, UserManagementView>()
			.AddSingleton<UserControl, BaseManagementView>();
	}
	
	
}
