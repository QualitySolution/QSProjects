using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.Commands;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using QS.Project.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace QS.Launcher;

public partial class App : Application {
	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public App(IEnumerable<ConnectionInfo> connectionInfos, LauncherOptions launcherOptions) {
		this.connectionInfos = connectionInfos;
		this.launcherOptions = launcherOptions;
	}

	IEnumerable<ConnectionInfo> connectionInfos;

	LauncherOptions launcherOptions;

	public App() : base() {}

	public override void OnFrameworkInitializationCompleted() {
		var collection = new ServiceCollection();

		collection.AddViewsAndViewModels();
		collection.AddNavigationCommands();

		collection.AddConnectionTypes(connectionInfos);
		collection.AddConnections(connectionInfos);
		collection.AddCompanyDependencies(launcherOptions);

		var services = collection.BuildServiceProvider();

		var mainWindow = services.GetRequiredService<MainWindow>();
		var mainWindowVM = services.GetRequiredService<MainWindowVM>();
		mainWindow.DataContext = mainWindowVM;

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = mainWindow;

		base.OnFrameworkInitializationCompleted();
	}
}

public static class ServiceCollectionExtensions {
	public static void AddViewsAndViewModels(this IServiceCollection collection) {
		collection.AddSingleton<MainWindowVM>();
		collection.AddSingleton<LoginVM>();
		collection.AddSingleton<DataBasesVM>();
		collection.AddSingleton<UserManagementVM>();
		collection.AddSingleton<BaseMagamenetVM>();

		collection.AddSingleton<MainWindow>();
		collection.AddSingleton<LoginView>();
		collection.AddSingleton<DataBasesView>();
		collection.AddSingleton<UserManagementView>();
		collection.AddSingleton<BaseManagementView>();
	}

	public static void AddNavigationCommands(this IServiceCollection collection) {
		collection.AddTransient<NextPageCommand>();
		collection.AddTransient<PreviousPageCommand>();
		collection.AddTransient<ChangePageCommand>();
	}

	public static void AddCompanyDependencies(this IServiceCollection collection, LauncherOptions launcherOptions) {
		collection.AddSingleton(launcherOptions);
	}

	public static void AddConnectionTypes(this IServiceCollection collection, IEnumerable<ConnectionInfo> connectionInfos) {
		foreach(var connectionInfo in connectionInfos)
			collection.AddSingleton(connectionInfo);
	}

	public static void AddConnections(this IServiceCollection collection, IEnumerable<ConnectionInfo> connectionInfos) {
		List<Dictionary<string, string>> connectionDefinitions;
		using (var stream = System.IO.File.Open("connections.json", System.IO.FileMode.OpenOrCreate)) {
			try {
				connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
			}
			catch (JsonException ex) {
				DialogWindow.Error(ex.Message, "Файл с подключениями некорректен");
				connectionDefinitions = [];
			}
		}


		foreach(var parameters in connectionDefinitions)
			collection.AddSingleton(
				connectionInfos.First(ci => ci.Title == parameters["Title"]).CreateConnection(parameters));
	}
}

