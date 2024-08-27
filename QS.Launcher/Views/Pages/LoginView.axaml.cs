using Avalonia.Controls;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using QS.Launcher.ViewModels.PageViewModels;

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
					TransformOperations.Parse($"translateY(-91px)"))
			},
		};

        InitializeComponent();

		loginContainer.Styles.Add(upStyle);

		DataContext = viewModel;
    }


	private void ShowCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		// if we change binding, transitions will not work
		createConnection.Classes.Remove("invisible");
		
		loginContainer.Classes.Remove("up");
	}

	private void HideCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		createConnection.Classes.Add("invisible");
		
		loginContainer.Classes.Add("up");
	}

	private void Border_SizeChanged(object? sender, SizeChangedEventArgs e)
	{
		((Setter)upStyle.Setters[0]).Value = TransformOperations.Parse($"translateY(-{(int)e.NewSize.Height}px)");
	}
}
