using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QS.Dialog;
using QS.Launcher.Views;
using ReactiveUI;
using System;
using System.Reactive;

namespace QS.Launcher;

public partial class LauncherApp() : Application
{
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	public Func<MainWindow> MainWindowGetter { get; set; }

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex => {
			logger.Error(ex, "Необработанная ошибка в ReactiveUI-команде.");
			new AvaloniaInteractiveMessage().ShowMessage(ImportanceLevel.Error, ex.Message, "Непредвиденная ошибка");
		});

		if (MainWindowGetter is null)
			throw new ArgumentNullException(nameof(MainWindowGetter));

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = MainWindowGetter();
		
		base.OnFrameworkInitializationCompleted();
	}
}
