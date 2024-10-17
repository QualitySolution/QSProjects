using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using Avalonia.Platform;
using Avalonia.Styling;
using QS.Launcher.ViewModels.PageViewModels;
using System.Globalization;

namespace QS.Launcher.Views.Pages;

public partial class LoginView : UserControl
{
	private readonly Style upStyle;

    public LoginView(LoginVM viewModel)
    {
		upStyle = new Style(x => x.OfType<ItemsControl>().Class("up")) {
			Setters =
			{
				new Setter(
					ItemsControl.RenderTransformProperty,
					TransformOperations.Parse($"translateY(-136px)"))
			},
		};

        InitializeComponent();

		loginContainer.Styles.Add(upStyle);

		DataContext = viewModel;

		KeyDown += (s, e) => {
			if(e.Key == Avalonia.Input.Key.Enter) {
				TopLevel.GetTopLevel(this).FocusManager.ClearFocus();
				viewModel.LoginCommand.Execute(null);
			}
		};
    }


	private void ShowCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		// if we change binding, transitions will not work
		createConnection.Classes.Remove("invisible");
		loginContainer.Classes.Remove("up");
		cogwheel.Classes.Remove("rolled");
	}

	private void HideCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.Classes.Add("invisible");
		loginContainer.Classes.Add("up");
		cogwheel.Classes.Add("rolled");
	}

	private void ToggleCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
		if(createConnection.Classes.Contains("invisible"))
			ShowCreationView(sender, e);
		else
			HideCreationView(sender, e);
	}

	private void Border_SizeChanged(object? sender, SizeChangedEventArgs e)
	{
		((Setter)upStyle.Setters[0]).Value = TransformOperations.Parse($"translateY(-{(int)e.NewSize.Height}px)");
		if (loginContainer.Classes.Contains("up")) {
			loginContainer.Classes.Remove("animated");
			loginContainer.Classes.Remove("up");
			loginContainer.Classes.Add("up");
			loginContainer.Classes.Add("animated");
		}
	}
}
