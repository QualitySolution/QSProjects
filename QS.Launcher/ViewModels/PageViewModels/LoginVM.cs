using System;
using QS.DbManagement;
using QS.Dialog;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class LoginVM : CarouselPageVM {
		private byte[] companyImage;
		public byte[] CompanyImage {
			get => companyImage;
			set => this.RaiseAndSetIfChanged(ref companyImage, value);
		}

		public string AppTitle { get; set; }

		public List<ConnectionTypeBase> ConnectionTypes { get; }

		private Connection selectedConnection;
		public Connection SelectedConnection {
			get => selectedConnection;
			set {
				this.RaiseAndSetIfChanged(ref selectedConnection, value);
				this.RaisePropertyChanged(nameof(CanLogin));
				this.RaisePropertyChanged(nameof(Connections));
				this.RaisePropertyChanged(nameof(SelectedConnection.CustomParameters));
			}
		}

		public ObservableCollection<Connection> Connections { get; set; }

		private string password;
		public string Password {
			get => password;
			set {
				this.RaiseAndSetIfChanged(ref password, value);
				this.RaisePropertyChanged(nameof(CanLogin));
				this.RaisePropertyChanged(nameof(IsPasswordEngOnly));
			}
		}

		public bool IsPasswordEngOnly => string.IsNullOrEmpty(Password) || 
			Password.Where(c => char.IsLetter(c)).All(c => c <= '~');

		protected IDbProvider dbProvider;

		public ICommand LoginCommand { get; }
		public ICommand AddCommand { get; }
		public ICommand NewCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand CloneCommand { get; }
		public ICommand SaveCommand { get; }

		protected IInteractiveMessage interactiveMessage;

		private readonly Configurator configurator;
		private readonly DataBasesVM dbVM;

		public LoginVM(
			IEnumerable<ConnectionTypeBase> connectionTypes,
			LauncherOptions options,
			Configurator configurator,
			DataBasesVM dbVM,
			IInteractiveMessage interactiveMessage) : base()
		{
			this.configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
			this.dbVM = dbVM;
			this.interactiveMessage = interactiveMessage;
			CompanyImage = options.LogoImage;
			AppTitle = options.AppTitle;

			ConnectionTypes = connectionTypes.ToList();
			Connections = new ObservableCollection<Connection>(configurator.ReadConnections()); 
			SelectedConnection = Connections.FirstOrDefault(c => c.Last);
			
			LoginCommand = ReactiveCommand.Create(Login);
			NewCommand = ReactiveCommand.Create(CreateNewConnection);
			DeleteCommand = ReactiveCommand.Create(DeleteSelectedConnection);
			CloneCommand = ReactiveCommand.Create(CloneConnection);
			SaveCommand = ReactiveCommand.Create(SaveConnections);
		}

		public void DeleteSelectedConnection() {
			Connections.Remove(SelectedConnection);
			SelectedConnection = null;
		}

		public void CloneConnection() {
			var n = SelectedConnection.Clone() as Connection;
			n.ConnectionTitle += "(копия)";
			Connections.Add(n);
			SelectedConnection = n;
		}

		public void CreateNewConnection() {
			Connection newCon = new Connection(ConnectionTypes.First(), new Dictionary<string, string>() {
				{"Title", "Новое подключение"}
			});
			Connections.Add(newCon);
			SelectedConnection = newCon;
		}

		public void Login() {
			if(SelectedConnection is null)
				return;
			
			dbProvider = SelectedConnection.CreateProvider(Password);
			var resp = dbProvider.LoginToServer();

			Task.Run(() => SaveCommand.Execute(null));

			if(resp.Success) {
				dbVM.SetProvider(dbProvider, SelectedConnection, SaveConnections);
				dbVM.IsAdmin = resp.IsAdmin;
				NextPageCommand?.Execute(null);
			}
			else
				interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Не удалось войти");
		}

		public bool CanLogin => SelectedConnection != null &&
					SelectedConnection.ConnectionType != null &&
					!string.IsNullOrWhiteSpace(Password) &&
					SelectedConnection.CanConnect();

		public void SaveConnections() {
			foreach(var conn in Connections)
				conn.Last = SelectedConnection == conn;
			
			configurator.SaveConnections(Connections);
		}
	}
}
