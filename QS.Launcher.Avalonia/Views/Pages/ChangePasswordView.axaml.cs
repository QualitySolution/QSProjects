using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;

public partial class ChangePasswordView : UserControl {
	public ChangePasswordView(ChangePasswordVM viewModel) {
		InitializeComponent();

		DataContext = viewModel;
	}
}
