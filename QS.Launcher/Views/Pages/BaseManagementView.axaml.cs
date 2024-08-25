using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using QS.Launcher.ViewModels.PageViewModels;
using System.Windows.Input;

namespace QS.Launcher.Views.Pages;

public partial class BaseManagementView : UserControl {
	public BaseManagementView(ICommand? nextPageCommand, ICommand? backPageCommand, ICommand? changePageCommand) {
		InitializeComponent();

		DataContext = new BaseMagamenetVM(nextPageCommand, backPageCommand, changePageCommand);
	}
}
