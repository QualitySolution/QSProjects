using Avalonia.Controls;
using Avalonia.Media.Imaging;
using QS.Launcher.ViewModels;
using System.Collections.Generic;
namespace QS.Launcher.Views;

public partial class MainWindow : Window {
	public MainWindow(MainWindowVM vm, IEnumerable<UserControl> pages, LauncherOptions options) {
		InitializeComponent();

		Icon = new WindowIcon(new Bitmap(typeof(MainWindowVM).Assembly.GetManifestResourceStream(options.CompanyIconName)));
		Title = options.AppTitle;

		Closing += (_, _) => vm.SaveConnections();

		foreach(var page in pages)
			carousel.Items.Add(page);
		vm.PagesCount = carousel.ItemCount;

		DataContext = vm;
	}
}
