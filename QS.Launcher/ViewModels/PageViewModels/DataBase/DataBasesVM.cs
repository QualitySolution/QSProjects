using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.Dialog;
using QS.Launcher.AppRunner;
using QS.Project.Versioning;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class DataBasesVM : CarouselPageVM {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private Connection currentConnection;
		private Action saveConnectionsAction;
		private IDbProvider provider;
		public IDbProvider Provider {
			get => provider;
			set {
				this.RaiseAndSetIfChanged(ref provider, value);
				Databases = provider.GetUserDatabases(applicationInfo).AsList();
				this.RaisePropertyChanged(nameof(Databases));

				LoadLastSelectedDatabase();
			}
		}

		public Connection CurrentConnection => currentConnection;

		public void SetProvider(IDbProvider dbProvider, Connection connection, Action saveConnections) {
			currentConnection = connection;
			saveConnectionsAction = saveConnections;
			Provider = dbProvider;
		}

		public List<DbInfo> Databases { get; set; }

		private DbInfo selectedDatabase;
		public DbInfo SelectedDatabase {
			get => selectedDatabase;
			set => this.RaiseAndSetIfChanged(ref selectedDatabase, value);
		}

		public bool IsAdmin { get; set; } = false;

		public bool ShouldCloseLauncherAfterStart { get; set; } = true;

		private readonly LauncherOptions launcherOptions;

		/// <summary>
		/// Указывает, должна ли быть видна галочка "Не закрывать лаунчер после подключения".
		/// Видна только в standalone режиме (когда лаунчер - отдельное приложение).
		/// </summary>
		public bool VisibleShouldCloseLauncherCheckBox => launcherOptions?.IsStandalone ?? false;

		public ICommand ConnectCommand { get; }
		public ReactiveCommand<Unit, Unit> OpenCreateDatabaseCommand { get; }

		public event Action<bool> StartLaunchProgram;

		IInteractiveMessage interactiveMessage;
		private readonly IServiceProvider serviceProvider;

		private readonly IAppRunner appRunner;
		private readonly IApplicationInfo applicationInfo;

		public DataBasesVM(
			IAppRunner appRunner,
			IApplicationInfo applicationInfo,
			IInteractiveMessage interactiveMessage,
			LauncherOptions launcherOptions,
			IServiceProvider serviceProvider)
		{
			this.appRunner = appRunner ?? throw new ArgumentNullException(nameof(appRunner));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.launcherOptions = launcherOptions;
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			IObservable<bool> canExecuteConnection = this
				.WhenAnyValue(x => x.SelectedDatabase)
				.Select(x => x != null);

			ConnectCommand = ReactiveCommand.Create(Connect, canExecuteConnection);
			OpenCreateDatabaseCommand = ReactiveCommand.Create(OpenCreateDatabase);
		}

		/// <summary>
		/// создаёт <see cref="CreateDataBaseSettingsVM"/> возвращает фокус на <see cref="DataBasesVM"/> и обновляет список баз
		/// </summary>
		private void OpenCreateDatabase() {
			if(provider == null || currentConnection == null)
				return;

			var settings = ActivatorUtilities.GetServiceOrCreateInstance<CreateDataBaseSettingsVM>(serviceProvider);
			settings.SetDbSettings(Provider, CurrentConnection);

			settings.ProgressPageRequested += progressVm => {
				progressVm.DatabaseCreated += OnDatabaseCreatedFromWizard;
				progressVm.DatabaseCreationFailed += () => {
					// пользователь сам решит вернуться или попробовать снова
				};
			};

			PushPageCommand?.Execute(settings);
		}

		private void OnDatabaseCreatedFromWizard() {
			// Закрываем все wizard-страницы и возвращаемся на DataBasesVM.
			PopToRootCommand?.Execute(null);
			RefreshDatabases();
		}

		public void RefreshDatabases() {
			if(provider == null) return;
			Databases = provider.GetUserDatabases(applicationInfo).AsList();
			this.RaisePropertyChanged(nameof(Databases));
			SelectedDatabase = Databases.FirstOrDefault();
			this.RaisePropertyChanged(nameof(SelectedDatabase));
		}

		private void LoadLastSelectedDatabase() {
			if(Databases == null || Databases.Count == 0)
				return;

			// Используем LastBaseId из текущего подключения
			if(currentConnection?.LastBaseId != null)
				SelectedDatabase = Databases.FirstOrDefault(db => db.BaseId == currentConnection.LastBaseId.Value);

			if(SelectedDatabase == null)
				SelectedDatabase = Databases.FirstOrDefault();
		}

		public void Connect() {

			var resp = provider.LoginToDatabase(SelectedDatabase);

			if(!resp.Success) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Ошибка подключения к базе данных");
				return;
			}

			SaveLastSelectedDatabase();

			var isStandalone = launcherOptions?.IsStandalone ?? false;
			logger.Info($">>> Connect: IsStandalone={isStandalone}, ShouldCloseLauncherAfterStart={ShouldCloseLauncherAfterStart}");

			bool shouldCloseLauncher = isStandalone && ShouldCloseLauncherAfterStart;

			logger.Info($">>> Connect: shouldCloseLauncher={shouldCloseLauncher}");

			StartLaunchProgram?.Invoke(shouldCloseLauncher);
			appRunner.Run(resp);
		}

		private void SaveLastSelectedDatabase() {
			if(SelectedDatabase == null || currentConnection == null)
				return;
			currentConnection.LastBaseId = SelectedDatabase.BaseId;

			saveConnectionsAction?.Invoke();
		}
	}
}
