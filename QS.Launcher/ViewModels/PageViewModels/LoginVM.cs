using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData.Kernel;
using QS.DbManagement;
using QS.Launcher.ViewModels.Commands;
using QS.Project.Avalonia;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels;

public class LoginVM : CarouselPageVM {
	private Bitmap companyImage;
	public Bitmap CompanyImage {
		get => companyImage;
		set => this.RaiseAndSetIfChanged(ref companyImage, value);
	}

	public List<ConnectionInfo> ConnectionTypes { get; }

	private Connection? selectedConnection;
	public Connection? SelectedConnection {
		get => selectedConnection;
		set {
			this.RaiseAndSetIfChanged(ref selectedConnection, value);
			this.RaisePropertyChanged(nameof(CanLogin));
		}
	}

	private Connection? newConnection;
	public Connection? NewConnection {
		get => newConnection;
		set {
			this.RaiseAndSetIfChanged(ref newConnection, value);
			this.RaisePropertyChanged(nameof(CanLogin));
		}
	}

	public ConnectionInfo? NewConnectionInfo {
		get {
			if (SelectedConnection != null)
				return SelectedConnection.ConnectionInfo.Clone() as ConnectionInfo;
			else return null;
		}
		set {
			if (value != null)
				SelectedConnection.ConnectionInfo = (ConnectionInfo)value.Clone();
			this.RaisePropertyChanged(nameof(SelectedConnection));
			this.RaisePropertyChanged(nameof(NewConnectionInfo));
			this.RaisePropertyChanged(nameof(CanLogin));
		}
	}

	public ObservableCollection<Connection> Connections { get; set; }

	//private string? user;
	//public string? User {
	//	get => user;
	//	set {
	//		this.RaiseAndSetIfChanged(ref user, value);
	//		this.RaisePropertyChanged(nameof(CanLogin));
	//	}
	//}

	private string? password;
	public string? Password {
		get => password;
		set {
			this.RaiseAndSetIfChanged(ref password, value);
			this.RaisePropertyChanged(nameof(CanLogin));
		}
	}

	protected IDbProvider dbProvider;

	public ICommand LoginCommand { get; }
	public ICommand AddCommand { get; }
	public ICommand NewCommand { get; }
	public ICommand DeleteCommand { get; }
	public ICommand CancelCommand { get; }

	private readonly DataBasesVM dbVM;

	public LoginVM(NextPageCommand? nextCommand, PreviousPageCommand? previousCommand, ChangePageCommand? changePageCommand,
		IEnumerable<ConnectionInfo> connectionInfos, IEnumerable<Connection> connections, LauncherOptions options, DataBasesVM dbVM)
		: base(nextCommand, previousCommand, changePageCommand) {
		this.dbVM = dbVM;
		CompanyImage = new Bitmap(AssetLoader.Open(options.CompanyImage));

		Connections = new(connections);

		ConnectionTypes = connectionInfos.AsList();


		LoginCommand = ReactiveCommand.Create(Login);
		NewCommand = ReactiveCommand.Create(CreateNewConnection);
		DeleteCommand = ReactiveCommand.Create(DeleteSelectedConnection);
		CancelCommand = ReactiveCommand.Create(CancelConnectionCreation);
	}

	public void ClearParamsInConnectionInfos() {
		foreach (var ci in ConnectionTypes) {
			ci.Parameters.ForEach(p => p.Value = null);
		}
		//NewConnectionInfo = null;
		this.RaisePropertyChanged(nameof(NewConnectionInfo));
	}

	public void DeleteSelectedConnection() {
		Connections.Remove(SelectedConnection);
		SelectedConnection = null;
		NewConnection = null;

		ClearParamsInConnectionInfos();
	}

	public void CloneConnection() {
		SelectedConnection = SelectedConnection.Clone() as Connection;
		if(Connections.Any(c => c.ConnectionTitle == NewConnection.ConnectionTitle)) {
			NewConnection.ConnectionTitle += "(копия)";
		}
		Connections.Add(SelectedConnection);
	}

	public void CancelConnectionCreation() {
		SelectedConnection = null;
	}

	public void CreateNewConnection() {
		Connection newCon = new();
		Connections.Add(newCon);
		SelectedConnection = newCon;
	}
	
	public void Login() {

		var usedConnection = SelectedConnection;

		if(usedConnection is null || usedConnection.ConnectionInfo is null)
			return;

		// TODO: solve the differences between ConnectionTypes and Connections
		if(dbProvider is null || dbProvider.ConnectionInfo == usedConnection.ConnectionInfo)
			dbProvider = usedConnection.ConnectionInfo.CreateProvider();

		var resp = dbProvider.LoginToServer(new LoginToServerData { UserName = usedConnection.User, Password = this.Password });

		if(resp.Success) {
			dbVM.Provider = dbProvider;
			dbVM.IsAdmin = resp.IsAdmin;
			NextPageCommand?.Execute(null);
		}
		else
			DialogWindow.Error(resp.ErrorMessage);
	}

	public bool CanLogin {
		get {
			var usedConnection = NewConnection ?? SelectedConnection;
			return
				usedConnection is not null &&
				usedConnection.ConnectionInfo is not null &&
				!string.IsNullOrWhiteSpace(Password) &&
				!string.IsNullOrWhiteSpace(usedConnection.User);
		}
	}
}
