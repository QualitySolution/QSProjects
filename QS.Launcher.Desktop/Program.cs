using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.ReactiveUI;
using QS.Cloud.Client;
using QS.DbManagement;

namespace QS.Launcher.Desktop;

/// <summary>
/// This class is for example purposes, usually project has its own Startup with DI-container for Launcher.
/// </summary>
class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => ConfigureApp()
        .StartWithClassicDesktopLifetime(args);

	public static AppBuilder ConfigureApp() {

		ConnectionInfo[] connectionInfos = [
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

		var connections = GetConnections();

		AppBuilder appBuilder = AppBuilder.Configure(() => new App(connectionInfos, connections))
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

		return appBuilder;
	}

	static List<Connection> GetConnections() {
		var connections = new List<Connection>();
		return connections;
	}
}
