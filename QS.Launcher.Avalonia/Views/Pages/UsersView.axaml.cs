using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;

public partial class UsersView : UserControl {
	public UsersView(UsersVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
