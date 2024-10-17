using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;
public partial class DataBasesView : UserControl {
	public DataBasesView(DataBasesVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
		viewModel.StartClosingLauncherEvent += () => (App.Current.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Shutdown();

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

	public void Label_PointerPressed(object? sender, PointerPressedEventArgs e) {
		if(!adminPanel.Classes.Remove("invisible"))
			adminPanel.Classes.Add("invisible");
	}
}
