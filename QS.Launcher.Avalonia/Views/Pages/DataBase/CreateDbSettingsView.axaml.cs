using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels.DataBase;

namespace QS.Launcher.Views.Pages.DataBase;

public partial class CreateDbSettingsView : UserControl {
	public CreateDbSettingsView(CreateDbSettingsVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
