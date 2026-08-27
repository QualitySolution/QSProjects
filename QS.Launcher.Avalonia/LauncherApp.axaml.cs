using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Launcher.Views;
using System;

namespace QS.Launcher;

public partial class LauncherApp() : Application
{
	public Func<MainWindow> MainWindowGetter { get; set; }

	/// <summary>Разбор ошибок для глобального перехватчика. Не задан - сообщение показывается без разбора</summary>
	public IErrorHandlingService ErrorHandling { get; set; }

	public override void Initialize() {
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted() {
		RxAppExceptionHandler.Install(ErrorHandling);
		DispatcherExceptionHandler.Install(ErrorHandling);

		if (MainWindowGetter is null)
			throw new ArgumentNullException(nameof(MainWindowGetter));

		if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			desktop.MainWindow = MainWindowGetter();
		
		base.OnFrameworkInitializationCompleted();
	}
}
