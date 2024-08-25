using Avalonia.Media.Imaging;
using Avalonia.Platform;
using QS.DbManagement;
using QS.Project.Avalonia;
using QS.Cloud.Client;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class LoginVM : CarouselPageVM {
	private Bitmap companyImage;
	public Bitmap CompanyImage {
		get => companyImage;
		set => this.RaiseAndSetIfChanged(ref companyImage, value);
	}

	public List<ConnectionInfo> ConnectionTypes { get; }

	private ConnectionInfo? selectedConnectionType;
	public ConnectionInfo? SelectedConnectionType {
		get => selectedConnectionType;
		set => this.RaiseAndSetIfChanged(ref selectedConnectionType, value);
	}

	private string? user;
	public string? User {
		get => user;
		set => this.RaiseAndSetIfChanged(ref user, value);
	}

	private string? password;
	public string? Password {
		get => password;
		set => this.RaiseAndSetIfChanged(ref password, value);
	}

	protected IDbProvider dbProvider;

	public ICommand LoginCommand { get; }

	public LoginVM(ICommand? nextCommand, ICommand? previousCommand, ICommand? changePageCommand)
		: base(nextCommand, previousCommand, changePageCommand)
	{
		CompanyImage = new Bitmap(AssetLoader.Open(new Uri("avares://QS.Launcher/Assets/sps.png")));

		ConnectionTypes =
		[
			new()
			{
				Title = "QS Cloud",
				IconBytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "qs.ico")),
				Parameters = [new ConnectionParameter("Логин")]
			},
			new()
			{
				Title = "MariaDB",
				IconBytes = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "mariadb.png")),
				Parameters = [new ConnectionParameter("Адрес")]
			},
			new()
			{
				Title = "SQLite",
				IconBytes = null,
				Parameters = [new ConnectionParameter("База данных"), new ConnectionParameter("Строка подключения")]
			}
		];

		var canExecute = this.WhenAnyValue(
			x => x.User, x => x.Password,
			(userName, password) =>
				!string.IsNullOrWhiteSpace(userName) &&
				!string.IsNullOrWhiteSpace(password));
		LoginCommand = ReactiveCommand.Create(Login, canExecute);
	}

	public void Login() {

		if(SelectedConnectionType is null)
			return;

		DbProviderFactory factory = new();

		if(SelectedConnectionType.Title == "QS Cloud")
			dbProvider = factory.CreateQSCloudProvider(SelectedConnectionType);
		else if(SelectedConnectionType.Title == "MariaDB")
			dbProvider = factory.CreateMariaDbProvider(SelectedConnectionType);

		try {
			if(dbProvider.LoginToServer(new LoginToServerData { UserName = User, Password = this.Password }))
				NextPageCommand?.Execute(null);
			else
				DialogWindow.Error("Не подключились");
		}
		catch(Exception ex) {

			DialogWindow.Error(ex.Message);
		}
	}

	public bool CanLogin => !string.IsNullOrWhiteSpace(Password) &&
		!string.IsNullOrWhiteSpace(User) && SelectedConnectionType is not null;
}
