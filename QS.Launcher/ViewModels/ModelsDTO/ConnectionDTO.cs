using QS.DbManagement;
using ReactiveUI;

namespace QS.Launcher.ViewModels.ModelsDTO;

internal class ConnectionDTO : ReactiveObject {

	string connectionTitle;
	public string ConnectionTitle {
		get => connectionTitle;
		set => this.RaiseAndSetIfChanged(ref connectionTitle, value);
	}

	string user;
	public string User {
		get => user;
		set => this.RaiseAndSetIfChanged(ref user, value);
	}

	ConnectionInfoDTO connectionInfo;
	public ConnectionInfoDTO ConnectionInfo {
		get => connectionInfo;
		set => this.RaiseAndSetIfChanged(ref connectionInfo, value);
	}



	public Connection Instance { get; }

	public ConnectionDTO(Connection connection) {
		Instance = connection;
		User = connection.User;
		ConnectionTitle = connection.ConnectionTitle;

		if (connection.ConnectionInfo != null)
			ConnectionInfo = new(connection.ConnectionInfo);
	}

	public ConnectionDTO() {
		
	}
}
