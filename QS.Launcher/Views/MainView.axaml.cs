using Avalonia.Controls;

namespace QS.Launcher.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

		//createConnection.IsVisible = false;
    }

	private void ShowCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.IsVisible = true;
	}

	private void HideCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.IsVisible = false;
	}
}
