using QS.DbManagement;
using ReactiveUI;
using System;

namespace QS.Launcher.ViewModels.ModelsDTO;

internal class ConnectionDTO : ReactiveObject, ICloneable {

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

	public bool Last { get; set; } = false;

	public Connection Instance { get; private set; }

	public ConnectionDTO(Connection connection) {
		Instance = connection;
		User = connection.User;
		ConnectionTitle = connection.ConnectionTitle;

		if (connection.ConnectionInfo != null)
			ConnectionInfo = new(connection.ConnectionInfo);

		Last = connection.Last;
	}

	public ConnectionDTO() {
		
	}

	public object Clone() {
		UpdateFields();
		return new ConnectionDTO(Instance.Clone() as Connection);
	}

	public void UpdateFields() {
		Instance ??= new();

		Instance.ConnectionTitle = ConnectionTitle;
		Instance.User = User;
		Instance.Last = Last;

		ConnectionInfo.UpdateFields();
		Instance.ConnectionInfo = ConnectionInfo.Instance;
	}


}
