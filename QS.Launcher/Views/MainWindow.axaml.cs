using Avalonia.Controls;
using QS.Launcher.ViewModels;
namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowVM viewModel)
    {
        InitializeComponent();

		DataContext = viewModel;
	}
}
