using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.Launcher.ViewModels;
using System.Collections.Generic;
namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow(IEnumerable<UserControl> pages, LauncherOptions options)
    {
        InitializeComponent();

		Icon = new WindowIcon(new Bitmap(AssetLoader.Open(options.CompanyIcon)));
		Title = options.AppTitle;

		Closing += (_, _) => ViewModel.SaveConnections();
		
		foreach (var page in pages)
			carousel.Items.Add(page);
		ViewModel.PagesCount = carousel.ItemCount;
	}

	MainWindowVM ViewModel => DataContext as MainWindowVM;

	public void NextPage() => ViewModel.NextPage();

	public void PreviousPage() => ViewModel.PreviousPage();

	public void ChangePage(int ind) => ViewModel.ChangePage(ind);
}
