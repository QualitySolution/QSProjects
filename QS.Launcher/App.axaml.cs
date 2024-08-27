using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.Commands;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.Views;
using QS.Launcher.Views.Pages;
using System.Windows.Input;
using System;

namespace QS.Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

		var collection = new ServiceCollection();
		collection.AddViewsAndViewModels();
		collection.AddNavigationCommands();
		var services = collection.BuildServiceProvider();

		var mainWindow = services.GetRequiredService<MainWindow>();


		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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

		//collection.AddSingleton<MainWindow>();
		collection.AddSingleton<LoginView>();
		collection.AddSingleton<DataBasesView>();
		collection.AddSingleton<UserManagementView>();
		collection.AddSingleton<BaseManagementView>();
	}

	public static void AddNavigationCommands(this IServiceCollection collection) {
		collection.AddSingleton<ICommand, NextPageCommand>();
		collection.AddSingleton<ICommand, PreviousPageCommand>();
		collection.AddSingleton<ICommand, ChangePageCommand>();
	}
}
