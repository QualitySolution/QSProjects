using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using QS.Launcher.ViewModels;

namespace QS.Launcher.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();

		DataContext = new LoginViewModel();
    }

	private void ShowCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.Classes.Remove("invisible");
		createConnection.IsEnabled = true;

		loginContainer.Classes.Remove("up");
	}

	private void HideCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.Classes.Add("invisible");
		loginContainer.Classes.Add("up");

		DoThingAfterDelay(() => createConnection.IsEnabled = false, 150);
		//Dispatcher.UIThread.Post(async () =>
		//{ 
		//	await Task.Delay(150);
		//	createConnection.IsEnabled = false;
		//});
	}

	public async void HideControlAfterDelay(Control control, int delayMilliseconds)
	{
		DoThingAfterDelay(() => control.IsVisible = false, delayMilliseconds);
	}

	public async void DoThingAfterDelay(Action action, int delayMilliseconds)
	{
		var t = Task.Delay(delayMilliseconds);

		action();
	}
}
