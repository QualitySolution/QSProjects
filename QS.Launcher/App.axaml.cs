using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Launcher.Views;
using System.Collections.Generic;

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

	public App() : base() { }

	public override void OnFrameworkInitializationCompleted() {
		var collection = new ServiceCollection();

		collection
			.AddPages()
			.AddViewModels();


		collection
			.AddConnectionTypes(connectionInfos)
			.AddConnections(connectionInfos, launcherOptions.OldConfigFilename)
			.AddCompanyDependencies(launcherOptions)
			.AddInteractive();

		var services = collection.BuildServiceProvider();

		var mainWindow = services.GetRequiredService<MainWindow>();

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = mainWindow;

		base.OnFrameworkInitializationCompleted();
	}
}
