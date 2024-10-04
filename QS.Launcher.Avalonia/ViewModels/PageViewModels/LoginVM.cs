using DynamicData.Kernel;
using QS.DbManagement;
using QS.Dialog;
using QS.Launcher.ViewModels.ModelsDTO;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels {
	// TODO: Deal with Bitmap
	public class LoginVM : CarouselPageVM {
		private byte[] companyImage;
		public byte[] CompanyImage {
			get => companyImage;
			set => this.RaiseAndSetIfChanged(ref companyImage, value);
		}

		public string AppTitle { get; set; }

		internal List<ConnectionInfoDTO> ConnectionTypes { get; }

		private ConnectionDTO? selectedConnection;
		internal ConnectionDTO? SelectedConnection {
			get => selectedConnection;
			set {
				this.RaiseAndSetIfChanged(ref selectedConnection, value);
				this.RaisePropertyChanged(nameof(CanLogin));
				this.RaisePropertyChanged(nameof(Connections));
				this.RaisePropertyChanged(nameof(NewConnectionInfo));
			}
		}

		internal ConnectionInfoDTO? NewConnectionInfo {
			get {
				if(SelectedConnection?.ConnectionInfo != null)
					return ConnectionTypes.First(ci => ci.Title == SelectedConnection.ConnectionInfo.Title);
				else return null;
			}
			set {
				if(value != null)
					SelectedConnection.ConnectionInfo = (ConnectionInfoDTO)value.Clone();
				this.RaisePropertyChanged(nameof(SelectedConnection));
				this.RaisePropertyChanged(nameof(NewConnectionInfo));
				this.RaisePropertyChanged(nameof(CanLogin));
			}
		}

		internal ObservableCollection<ConnectionDTO> Connections { get; set; }

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
		public ICommand CloneCommand { get; }
		public ICommand SaveCommand { get; }

		protected IInteractiveMessage interactiveMessage;

		readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

		private readonly DataBasesVM dbVM;

		public LoginVM(IEnumerable<ConnectionInfo> connectionInfos, IEnumerable<Connection> connections, LauncherOptions options,
			DataBasesVM dbVM, IInteractiveMessage interactiveMessage) : base()
		{
			this.dbVM = dbVM;
			this.interactiveMessage = interactiveMessage;
			CompanyImage = new Bitmap(AssetLoader.Open(options.CompanyImage));
			AppTitle = options.AppTitle;

			Connections = new(connections.Select(c => new ConnectionDTO(c)));

			SelectedConnection = Connections.FirstOrDefault(c => c.Last);

			ConnectionTypes = connectionInfos.Select(ci => new ConnectionInfoDTO(ci)).AsList();


			LoginCommand = ReactiveCommand.Create(Login);
			NewCommand = ReactiveCommand.Create(CreateNewConnection);
			DeleteCommand = ReactiveCommand.Create(DeleteSelectedConnection);
			CloneCommand = ReactiveCommand.Create(CloneConnection);
			SaveCommand = ReactiveCommand.Create(SerialaizeConnections);
		}

		public void DeleteSelectedConnection() {
			Connections.Remove(SelectedConnection);
			SelectedConnection = null;
		}

		public void CloneConnection() {
			var n = SelectedConnection.Clone() as ConnectionDTO;
			n.ConnectionTitle += "(копия)";
			n.UpdateFields();
			Connections.Add(n);
			SelectedConnection = n;
		}

		public void CreateNewConnection() {
			ConnectionDTO newCon = new();
			Connections.Add(newCon);
			SelectedConnection = newCon;
		}

		public void Login() {

			var usedConnection = SelectedConnection;

			if(usedConnection is null || usedConnection.ConnectionInfo is null)
				return;

			usedConnection.ConnectionInfo.UpdateFields();
			if(dbProvider is null || dbProvider.ConnectionInfo.Title == usedConnection.ConnectionInfo.Title)
				dbProvider = usedConnection.ConnectionInfo.Instance.CreateProvider();

			var resp = dbProvider.LoginToServer(new LoginToServerData { UserName = usedConnection.User, Password = this.Password });

			Task.Run(() => SaveCommand.Execute(null));

			if(resp.Success) {
				dbVM.Provider = dbProvider;
				dbVM.IsAdmin = resp.IsAdmin;
				NextPageCommand?.Execute(null);
			}
			else
				interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Не удалось войти");
		}

		public bool CanLogin {
			get {
				var usedConnection = SelectedConnection;
				return
					usedConnection is not null &&
					usedConnection.ConnectionInfo is not null &&
					!string.IsNullOrWhiteSpace(Password) &&
					!string.IsNullOrWhiteSpace(usedConnection.User);
			}
		}

		public void SerialaizeConnections() {
			// if last selected connection changed
			if(SelectedConnection != null && !SelectedConnection.Last) {
				var prevLast = Connections.FirstOrDefault(c => c.Last);
				if(prevLast != null)
					prevLast.Last = false;
				SelectedConnection.Last = true;
			}
			foreach(var conn in Connections)
				conn.UpdateFields();

			using(FileStream stream = new("connections.json", FileMode.Create)) {
				JsonSerializer.Serialize(stream, Connections
					.Select(c => c.Instance.PrepareParams())
					.ToList(), serializerOptions);
			}
		}
	}
}
