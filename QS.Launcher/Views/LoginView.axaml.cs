using Avalonia.Controls;
using System;
using System.Threading.Tasks;
using QS.Launcher.ViewModels;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace QS.Launcher.Views;

public partial class LoginView : UserControl
{
	private readonly Style upStyle;

    public LoginView(ICommand? nextPageCommand, ICommand? backPageCommand)
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

		DataContext = new LoginViewModel(nextPageCommand, backPageCommand);
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

	private void Border_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
	{
		((Setter)upStyle.Setters[0]).Value = TransformOperations.Parse($"translateY(-{(int)e.NewSize.Height}px)");
	}
}
