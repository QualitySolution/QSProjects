using Avalonia.Controls;
using Avalonia.Media.Imaging;
using QS.Launcher.ViewModels;

namespace QS.Launcher.Views;

public partial class MainWindow : Window {
	public MainWindow(MainWindowVM vm, LauncherOptions options) {
		InitializeComponent();

		Icon = new WindowIcon(new Bitmap(new System.IO.MemoryStream(options.LogoIcon)));
		Title = options.AppTitle;

		Closing += (_, _) => vm.SaveConnections();

		DataContext = vm;
	}
}
