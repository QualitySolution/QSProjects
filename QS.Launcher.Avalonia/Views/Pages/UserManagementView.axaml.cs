using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;

public partial class UserManagementView : UserControl {
	public UserManagementView(UserManagementVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
