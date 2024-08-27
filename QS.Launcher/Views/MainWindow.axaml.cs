using Avalonia.Controls;
using QS.Launcher.ViewModels;
namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
	}

	MainWindowVM ViewModel => DataContext as MainWindowVM;

	public void NextPage() => ViewModel.NextPage();

	public void PreviousPage() => ViewModel.PreviousPage();

	public void ChangePage(int ind) => ViewModel.ChangePage(ind);
}
