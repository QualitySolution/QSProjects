using ReactiveUI;
using System.Collections.Generic;

namespace QS.Launcher.ViewModels;

public class MainViewModel : ViewModelBase
{
	public string Greeting => "Welcome to Avalonia!";

	public List<ConnectionTypeViewModel> ConnectionTypes { get; } = new();

	public string password;
	public string Password
	{
		get => password;
		set => this.RaiseAndSetIfChanged(ref password, value);
	}

	public MainViewModel()
	{
		ConnectionTypes = new List<ConnectionTypeViewModel>
		{
			new() { Title = "QS Cloud", IconPath = "Assets/avalonia-logo.ico" },
			new() { Title = "MariaDB", IconPath = "Assets/avalonia-logo.ico" }
		};
	}
}
