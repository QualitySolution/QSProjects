using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Dialog;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using QS.Project.Avalonia;
using QS.Project.Avalonia.Interactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

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

	public static IServiceCollection AddInteractive(this IServiceCollection services) {
		return services
			.AddSingleton<IInteractiveMessage, AvaloniaInteractiveMessage>();
	}
}
