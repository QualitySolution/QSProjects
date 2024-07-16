using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace QS.Launcher.ViewModels;

public class LoginViewModel : PageViewModel
{
	private Bitmap companyImage;
	public Bitmap CompanyImage
	{
		get => companyImage;
		set => this.RaiseAndSetIfChanged(ref companyImage, value);
	}

	public List<ConnectionTypeViewModel> ConnectionTypes { get; }

	public string password;
	public string Password
	{
		get => password;
		set => this.RaiseAndSetIfChanged(ref password, value);
	}

	public LoginViewModel(ICommand nextCommand, ICommand previousCommand) : base(nextCommand, previousCommand)
	{
		CompanyImage = new Bitmap(AssetLoader.Open(new Uri("avares://QS.Launcher/Assets/avalonia-logo.ico")));

		ConnectionTypes =
		[
			new() { Title = "QS Cloud", Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico")) },
			new() { Title = "MariaDB", Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")) }
		];
	}
}
