using Avalonia.Controls;
using QS.Launcher.ViewModels.PageViewModels;

namespace QS.Launcher.Views.Pages;

public partial class BaseManagementView : UserControl {
	public BaseManagementView(BaseManagementVM viewModel) {
		InitializeComponent();
		DataContext = viewModel;
	}
}
