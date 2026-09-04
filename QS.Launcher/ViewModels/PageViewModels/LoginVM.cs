using QS.DbManagement;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.Project.Versioning;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class LoginVM : CarouselPageVM {
		private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
		private readonly IApplicationInfo applicationInfo;

		public ICommand LoginCommand { get; }
		public ICommand AddCommand { get; }
		public ICommand NewCommand { get; }
		public ICommand DeleteCommand { get; }
		public ICommand CloneCommand { get; }
		public ICommand SaveCommand { get; }

		protected IInteractiveMessage interactiveMessage;

		private readonly Configurator configurator;
		private readonly DataBasesVM dbVM;
		private readonly IErrorHandlingService errorHandling;

		public LoginVM(
			LauncherNavigation navigation,
			LauncherOptions options,
			Configurator configurator,
			IApplicationInfo applicationInfo,
			DataBasesVM dbVM,
			IInteractiveMessage interactiveMessage,
			IErrorHandlingService errorHandling) : base(navigation)
		{
			this.configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
			this.dbVM = dbVM;
			this.interactiveMessage = interactiveMessage;
			this.errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));
			CompanyImage = options.LogoImage;
			AppTitle = options.AppTitle;

			this.applicationInfo = applicationInfo;
			// типы подключений берём у конфигуратора: он же по ним и разбирает файл,
			// и второй список тех же типов означал бы два источника правды
			ConnectionTypes = configurator.ConnectionTypes.ToList();
			Connections = new ObservableCollection<Connection>(configurator.ReadConnections());
			SelectedConnection = Connections.FirstOrDefault(c => c.Last);
			
			LoginCommand = ReactiveCommand.CreateFromTask(Login);
			NewCommand = ReactiveCommand.Create(CreateNewConnection);
			DeleteCommand = ReactiveCommand.Create(DeleteSelectedConnection);
			CloneCommand = ReactiveCommand.Create(CloneConnection);
			SaveCommand = ReactiveCommand.Create(SaveConnections);

			TrackBusy(LoginCommand);
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

		public async Task Login() {
			var connection = SelectedConnection;
			if(connection is null)
				return;

			try {
				dbProvider = connection.CreateProvider(Password, applicationInfo.ProductCode);
				var resp = await Task.Run(() => dbProvider.LoginToServer());
				SaveConnections();

				if(resp.Success) {
					await dbVM.SetProviderAsync(dbProvider, connection, SaveConnections);
					Navigation.Next();
				}
				else
					interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Не удалось войти");
			}
			catch(Exception ex) {
				errorHandling.Handle(ex, "Не удалось войти");
			}
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
