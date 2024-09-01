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
using QS.Launcher.ViewModels.Commands;
using DynamicData.Kernel;
using System.Collections.ObjectModel;

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

	public ObservableCollection<Connection> Connections { get; set; }

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

	private DataBasesVM dbVM;

	public LoginVM(NextPageCommand? nextCommand, PreviousPageCommand? previousCommand, ChangePageCommand? changePageCommand,
		IEnumerable<ConnectionInfo> connectionInfos, IEnumerable<Connection> connections, Uri companyLogoUri, DataBasesVM dbVM)
		: base(nextCommand, previousCommand, changePageCommand)
	{
		this.dbVM = dbVM;
		CompanyImage = new Bitmap(AssetLoader.Open(companyLogoUri));

		Connections = new(connections);

		ConnectionTypes = connectionInfos.AsList();

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

		// TODO: solve the differences between ConnectionTypes and Connections
		if (dbProvider is null || dbProvider.Connection == SelectedConnectionType)
			dbProvider = SelectedConnectionType.CreateProvider();

		var resp = dbProvider.LoginToServer(new LoginToServerData { UserName = User, Password = this.Password });

		if(resp.Success) {
			dbVM.Provider = dbProvider;
			dbVM.IsAdmin = resp.IsAdmin;
			NextPageCommand?.Execute(null);
		}
		else
			DialogWindow.Error(resp.ErrorMessage);
	}

	public bool CanLogin => !string.IsNullOrWhiteSpace(Password) &&
		!string.IsNullOrWhiteSpace(User) && SelectedConnectionType is not null;
}
