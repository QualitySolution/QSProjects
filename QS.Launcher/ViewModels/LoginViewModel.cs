using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.DbManagement;
using QS.Launcher.ViewModels.TypesViewModels;
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

	public List<ConnectionInfo> ConnectionTypes { get; }

	private ConnectionInfo selectedConnectionType;
	public ConnectionInfo SelectedConnectionType
	{
		get => selectedConnectionType;
		set => this.RaiseAndSetIfChanged(ref selectedConnectionType, value);
	}

	private string password;
	public string Password
	{
		get => password;
		set => this.RaiseAndSetIfChanged(ref password, value);
	}

	private double transformHeight;
	public double TransformHeight
	{
		get => transformHeight;
		set => this.RaiseAndSetIfChanged(ref transformHeight, value);
	}

	public LoginViewModel(ICommand nextCommand, ICommand previousCommand) : base(nextCommand, previousCommand)
	{
		CompanyImage = new Bitmap(AssetLoader.Open(new Uri("avares://QS.Launcher/Assets/sps.png")));

		ConnectionTypes =
		[
			new()
			{
				Title = "QS Cloud",
				Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico")),
				Parameters = [new ConnectionParameter("Логин")]
			},
			new()
			{
				Title = "MariaDB",
				Icon = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")),
				Parameters = [new ConnectionParameter("Адрес")]
			},
			new()
			{
				Title = "SQLite",
				Icon = null,
				Parameters = [new ConnectionParameter("База данных"), new ConnectionParameter("Строка подключения")]
			}
		];
	}
}
