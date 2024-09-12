using System;
using Avalonia;
using Avalonia.ReactiveUI;
using QS.Cloud.Client;
using QS.DbManagement;

namespace QS.Launcher.Desktop;

/// <summary>
/// This class is for example purposes, usually project has its own Startup with DI-container for Launcher.
/// </summary>
class ExampleStartup
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => ConfigureApp()
        .StartWithClassicDesktopLifetime(args);

	public static AppBuilder ConfigureApp() {

		ConnectionInfo[] connectionInfos = [
			new QSCloudConnectionInfo([new ConnectionParameter("Логин")]){
				Title = "QS Cloud",
				IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico"))
			},
			new MariaDBConnectionInfo([new ConnectionParameter("Адрес")]) {
				Title = "MariaDB",
				IconBytes = System.IO.File.ReadAllBytes(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")),
			},
			new ExampleConnectionInfo([new ConnectionParameter("База данных"), new ConnectionParameter("Строка подключения")]) {
				Title = "SQLite",
				IconBytes = null,
		} ];

		var options = new LauncherOptions {
			AppTitle = "Ланучер для детей",
			CompanyImage = new Uri("avares://QS.Launcher/Assets/sps.png"),
			CompanyIcon = new Uri("avares://QS.Launcher/Assets/sps1024.png")
		};

		AppBuilder appBuilder = AppBuilder.Configure(() => new App(connectionInfos, options))
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();

		return appBuilder;
	}
}
