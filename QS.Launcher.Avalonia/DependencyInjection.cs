using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using QS.Launcher.Views.Pages.DataBase;

namespace QS.Launcher;
public static partial class DependencyInjection {
	public static IServiceCollection AddPages(this IServiceCollection services) {
		return services
			.AddSingleton<MainWindow>()
			.AddSingleton<PageViewLocator>()
			.AddSingleton<IGuiDispatcher, AvaloniaGuiDispatcher>();
	}
}
