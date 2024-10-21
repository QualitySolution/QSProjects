using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using QS.Launcher.ViewModels.PageViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace QS.Launcher.Views.Pages;
public partial class DataBasesView : UserControl {

	public DataBasesView(DataBasesVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;

		viewModel.StartLaunchProgram += HandleStartMainProgram;

		KeyDown += (s, e) => {
			if(e.Key == Key.Enter) {
				TopLevel.GetTopLevel(this).FocusManager.ClearFocus();
				viewModel.ConnectCommand.Execute(null);
			}
		};

		databases.DoubleTapped += (s, e) => {
			if(databases.SelectedItem is not null)
				viewModel.ConnectCommand.Execute(null);
		};
	}

	public async void HandleStartMainProgram(bool shouldCloseLauncher) {
		loadingPanel.IsVisible = true;
		cogwheel.Classes.Add("rolled");

		var transition = cogwheel.Transitions.OfType<TransformOperationsTransition>().FirstOrDefault();
		await Task.Delay(transition.Duration);
		loadingPanel.IsVisible = false;
		if(shouldCloseLauncher)
			(LauncherApp.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();
		else
			cogwheel.Classes.Remove("rolled");
	}

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e) {
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}
}
