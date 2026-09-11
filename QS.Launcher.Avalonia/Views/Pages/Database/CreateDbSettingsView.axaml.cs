using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels.Database;

namespace QS.Launcher.Views.Pages.Database;

public partial class CreateDbSettingsView : UserControl {
	public CreateDbSettingsView(CreateDbSettingsVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
