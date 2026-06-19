using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using QS.DbManagement;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Views.Pages;
public partial class DataBasesView : UserControl {
	private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

	private readonly DataBasesVM ViewModel;

	public DataBasesView(DataBasesVM viewModel) {
		InitializeComponent();

		DataContext = ViewModel = viewModel;

		ViewModel.StartLaunchProgram += HandleStartMainProgram;

		KeyDown += (s, e) => {
			if(e.Key == Key.Enter) {
				TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
				ViewModel.ConnectCommand.Execute(null);
			}
		};
	}

	public async void HandleStartMainProgram(bool shouldCloseLauncher) {
		logger.Info($">>> HandleStartMainProgram: shouldCloseLauncher={shouldCloseLauncher}");

		loadingPanel.IsVisible = true;
		cogwheel.Classes.Add("rolled");

		var transition = cogwheel.Transitions.OfType<TransformOperationsTransition>().FirstOrDefault();
		if(transition != null)
			await Task.Delay(transition.Duration);
		loadingPanel.IsVisible = false;

		if(shouldCloseLauncher) {
			logger.Info($">>> HandleStartMainProgram: Вызываем Shutdown!");
			// NewProcessRunner: закрываем всё приложение лаунчера (Shutdown)
			(LauncherApp.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();
		}
		else {
			logger.Info($">>> HandleStartMainProgram: Закрываем только окно");
			// InProcessRunner: закрываем только окно лаунчера
			var window = TopLevel.GetTopLevel(this) as Window;
			window?.Close();
		}
	}

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e) {
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}

	private void Databases_OnDoubleTapped(object? sender, TappedEventArgs e) {
		if(databases.SelectedItem is not null)
			ViewModel.ConnectCommand.Execute(null);
	}

	private void BackupDatabase_OnClick(object? sender, RoutedEventArgs e) {
		if((sender as Control)?.DataContext is DbInfo database)
			ViewModel.BackupDatabaseCommand.Execute(database);
	}

	private void DeleteDatabase_OnClick(object? sender, RoutedEventArgs e) {
		if((sender as Control)?.DataContext is DbInfo database)
			ViewModel.DeleteDatabaseCommand.Execute(database);
	}
}
