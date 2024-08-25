using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QS.Launcher.ViewModels.PageViewModels;
using System.Windows.Input;

namespace QS.Launcher.Views.Pages;

public partial class UserManagementView : UserControl {
	public UserManagementView(ICommand? nextPageCommand, ICommand? backPageCommand, ICommand? changePageCommand) {
		InitializeComponent();

		DataContext = new UserManagementVM(nextPageCommand, backPageCommand, changePageCommand);
	}
}
