using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.Commands;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using System;
using QS.DbManagement;
using QS.Cloud.Client;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace QS.Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
		CreateTestConfigs();
		var c = Configure();
		var con = c.GetValue<string>("ExampleConnectionInfo:Title");

		var collection = new ServiceCollection();

		collection.AddViewsAndViewModels();
		collection.AddNavigationCommands();

		collection.AddTestConnectionTypes();
		collection.AddCompanyDependencies();

		var services = collection.BuildServiceProvider();

		var mainWindow = services.GetRequiredService<MainWindow>();
		var mainWindowVM = services.GetRequiredService<MainWindowVM>();
		mainWindow.DataContext = mainWindowVM;
		
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = mainWindow;

        base.OnFrameworkInitializationCompleted();
    }

	IConfiguration configurationJson = new ConfigurationBuilder()
		.AddJsonFile("launcher.json")
		.Build();
	IConfiguration configurationEnv = new ConfigurationBuilder()
		.AddEnvironmentVariables("QS_")
		.Build();

	public void CreateTestConfigs() {
		var conSec = configurationJson.GetSection("Connections");
		string json = JsonSerializer.Serialize(connections);
		using(var s = new System.IO.FileStream("launcher.json", System.IO.FileMode.OpenOrCreate)) {
			JsonSerializer.Serialize(s, connections, new JsonSerializerOptions { WriteIndented = true });
		}
	}

	ConnectionInfo[] connections = [
		new QSCloudConnectionInfo {
			Title = "QS Cloud",
			IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico")),
			Parameters = [new ConnectionParameter("Логин")]
		},
		new MariaDBConnectionInfo {
			Title = "MariaDB",
			IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")),
			Parameters = [new ConnectionParameter("Адрес")]
		},
		new ExampleConnectionInfo {
			Title = "SQLite",
			IconBytes = null,
			Parameters = [new ConnectionParameter("База данных"), new ConnectionParameter("Строка подключения")]
	} ];

	public IConfiguration Configure() {

		return configurationJson.GetSection("Connections");
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

	public static void AddCompanyDependencies(this IServiceCollection collection) {
		collection.AddTransient(sp => new Uri("avares://QS.Launcher/Assets/sps.png"));
	}

	public static void AddTestConnectionTypes(this IServiceCollection collection) {
		collection.AddTransient<ConnectionInfo, QSCloudConnectionInfo>(sp => new QSCloudConnectionInfo {
			Title = "QS Cloud",
			IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico")),
			Parameters = [new ConnectionParameter("Логин")]
		});
		collection.AddTransient<ConnectionInfo, MariaDBConnectionInfo>(sp => new MariaDBConnectionInfo {
			Title = "MariaDB",
			IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")),
			Parameters = [new ConnectionParameter("Адрес")]
		});
		collection.AddTransient<ConnectionInfo, ExampleConnectionInfo>(sp => new ExampleConnectionInfo {
			Title = "SQLite",
			IconBytes = null,
			Parameters = [new ConnectionParameter("База данных"), new ConnectionParameter("Строка подключения")]
		});
		collection.AddTransient<ConnectionInfo, ExampleConnectionInfo>();
	}
}

