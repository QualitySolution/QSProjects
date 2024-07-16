using Avalonia.Controls;
using QS.Launcher.ViewModels;
namespace QS.Launcher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

		DataContext = new MainWindowViewModel(carousel);
	}

	private void Next(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		carousel.Next();
	}

	private void Back(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		carousel.Previous();
	}
}
