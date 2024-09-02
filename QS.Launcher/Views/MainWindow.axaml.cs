using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.Launcher.ViewModels;
namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow(LauncherOptions options)
    {
        InitializeComponent();

		Icon = new WindowIcon(new Bitmap(AssetLoader.Open(options.CompanyIcon)));
		Title = options.AppTitle;
	}

	MainWindowVM ViewModel => DataContext as MainWindowVM;

	public void NextPage() => ViewModel.NextPage();

	public void PreviousPage() => ViewModel.PreviousPage();

	public void ChangePage(int ind) => ViewModel.ChangePage(ind);
}
