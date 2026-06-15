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
				this.RaisePropertyChanged(nameof(CanCreateDatabase));
				this.RaisePropertyChanged(nameof(CanManageDatabases));

				LoadLastSelectedDatabase();
			}
		}

		/// <summary>
		/// можно создать базу только если:
		/// есть права пользователя на создание
		/// тип подключения поддерживает создание в текущем окружении
		/// задана фабрика и зарегистрирован скрипт создания
		/// </summary>
		public bool CanCreateDatabase =>
			provider != null
			&& provider.CanCreateDatabase
			&& currentConnection?.ConnectionType?.SupportsDatabaseCreation(serviceProvider) == true;

		/// <summary>
		/// Управление базой (резервная копия, удаление) пока поддержано только для MariaDB-провайдера,
		/// облако реализуем отдельно.
		/// </summary>
		public bool CanManageDatabases => provider is MariaDBProvider;

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
		public ICommand BackupDatabaseCommand { get; }
		public ICommand DeleteDatabaseCommand { get; }

		public event Action<bool> StartLaunchProgram;

		IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IServiceProvider serviceProvider;

		private readonly IAppRunner appRunner;
		private readonly IApplicationInfo applicationInfo;

		public DataBasesVM(
			IAppRunner appRunner,
			IApplicationInfo applicationInfo,
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			LauncherOptions launcherOptions,
			IServiceProvider serviceProvider)
		{
			this.appRunner = appRunner ?? throw new ArgumentNullException(nameof(appRunner));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.launcherOptions = launcherOptions;
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			IObservable<bool> canExecuteConnection = this
				.WhenAnyValue(x => x.SelectedDatabase)
				.Select(x => x != null);

			ConnectCommand = ReactiveCommand.Create(Connect, canExecuteConnection);
			OpenCreateDatabaseCommand = ReactiveCommand.Create(OpenCreateDatabase);
			BackupDatabaseCommand = ReactiveCommand.Create<DbInfo>(OpenBackup);
			DeleteDatabaseCommand = ReactiveCommand.CreateFromTask<DbInfo>(DeleteDatabaseAsync);
		}

		/// <summary>
		/// Мастер настроек/прогресса един для всех операций с базой - резолвим один раз
		/// и один раз подписываемся на завершение операции (без накопления подписок).
		/// </summary>
		private CreateDataBaseSettingsVM settingsWizard;
		private CreateDataBaseSettingsVM SettingsWizard {
			get {
				if(settingsWizard == null) {
					settingsWizard = ActivatorUtilities.GetServiceOrCreateInstance<CreateDataBaseSettingsVM>(serviceProvider);
					settingsWizard.ProgressPageRequested += progressVm => {
						progressVm.OperationCompleted -= OnWizardOperationCompleted;
						progressVm.OperationCompleted += OnWizardOperationCompleted;
					};
				}
				return settingsWizard;
			}
		}

		/// <summary>
		/// открывает мастер создания базы, по завершении возвращает фокус на <see cref="DataBasesVM"/> и обновляет список баз
		/// </summary>
		private void OpenCreateDatabase() {
			if(!CanCreateDatabase)
				return;

			SettingsWizard.SetDbSettings(Provider, CurrentConnection);
			PushPageCommand?.Execute(SettingsWizard);
		}

		/// <summary>
		/// открывает мастер резервного копирования выбранной базы
		/// </summary>
		private void OpenBackup(DbInfo database) {
			if(database == null || !CanManageDatabases)
				return;

			SettingsWizard.SetBackupSettings(Provider, CurrentConnection, database);
			PushPageCommand?.Execute(SettingsWizard);
		}

		private void OnWizardOperationCompleted() {
			// Закрываем все wizard-страницы и возвращаемся на DataBasesVM.
			PopToRootCommand?.Execute(null);
			RefreshDatabases();

			if(settingsWizard?.Operation == DbWizardOperation.Backup)
				interactiveMessage.ShowMessage(ImportanceLevel.Success,
					$"Резервная копия базы данных сохранена:\n{settingsWizard.BackupFilePath}",
					"Резервное копирование");
		}

		private async System.Threading.Tasks.Task DeleteDatabaseAsync(DbInfo database) {
			if(database == null || !CanManageDatabases)
				return;

			// Question() кидает исключение на UI-потоке, поэтому диалог и удаление выполняем в фоне.
			bool confirmed = await System.Threading.Tasks.Task.Run(() => interactiveQuestion.Question(
				$"Безвозвратно удалить базу данных «{database.Title}»?", "Удаление базы данных"));
			if(!confirmed)
				return;

			try {
				await System.Threading.Tasks.Task.Run(() => provider.DropDatabase(database.BaseName));
				RefreshDatabases();
				interactiveMessage.ShowMessage(ImportanceLevel.Success,
					$"База данных «{database.Title}» удалена.", "Удаление базы данных");
			}
			catch(Exception ex) {
				logger.Error(ex, "Не удалось удалить базу {0}", database.BaseName);
				interactiveMessage.ShowMessage(ImportanceLevel.Error, ex.Message, "Ошибка удаления базы данных");
			}
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

			// Определяем, нужно ли закрывать лаунчер через Shutdown
			// В standalone режиме учитываем галочку ShouldCloseLauncherAfterStart
			// В in-process режиме НЕ делаем shutdown (возвращаем false)
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
