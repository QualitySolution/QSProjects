using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
using System.IO;
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
		collection.AddConnections(connectionInfos, launcherOptions.OldConfigFilename);
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


	public static void AddConnections(this IServiceCollection collection, IEnumerable<ConnectionInfo> connectionInfos, string? oldConfigName = null) {
		List<Dictionary<string, string>> connectionDefinitions = [];
		try {
			using (var stream = File.Open("connections.json", FileMode.Open)) {
				try {
					connectionDefinitions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(stream);
				}
				catch (JsonException jsonEx) {
					DialogWindow.Error(jsonEx.Message, "Файл с подключениями некорректен");
				}
			}
		}
		catch(FileNotFoundException fileEx) {
			// Trying to find old ini-config and update it to new json-config
			if (!string.IsNullOrEmpty(oldConfigName)) {
				
				string fullOldConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), oldConfigName);

				if (File.Exists(fullOldConfigPath)) {
					connectionDefinitions = Configuration.ConfigTransition.FromOldIniConfig(fullOldConfigPath);
					DialogWindow.Info("Найден файл с подключениями старой версии, будет создан новый на его основе", "Конфигурация обновлена");
				}
				else
					DialogWindow.Info("Будет создан новый", "Файл с подключениями не найден");
				
			}
		}


		foreach(var parameters in connectionDefinitions)
			collection.AddSingleton(
				connectionInfos.First(ci => ci.Title == parameters["Title"]).CreateConnection(parameters));
	}
}

