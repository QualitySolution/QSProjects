using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QS.Launcher.Views;
using System;

namespace QS.Launcher;

public partial class LauncherApp() : Application
{
	public Func<MainWindow> MainWindowGetter { get; set; }

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		if (MainWindowGetter is null)
			throw new ArgumentNullException(nameof(MainWindowGetter));

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = MainWindowGetter();
		
		base.OnFrameworkInitializationCompleted();
	}
}
