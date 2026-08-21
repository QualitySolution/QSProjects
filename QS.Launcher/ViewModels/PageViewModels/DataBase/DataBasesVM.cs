using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using QS.ErrorReporting;
using QS.Launcher.AppRunner;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Project.Versioning;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class DataBasesVM : CarouselPageVM {
		private const string DropDatabaseTitle = "Удаление базы данных";
		private const string RefreshMetadataTitle = "Синхронизация метаинформации";

		private Connection currentConnection;
		private Action saveConnectionsAction;

		public IDbProvider Provider { get; private set; }

		private DbCapabilitySet capabilitySet = DbCapabilitySet.None;
		public DbCapabilitySet Capabilities {
			get => capabilitySet;
			private set => this.RaiseAndSetIfChanged(ref capabilitySet, value);
		}

		public Connection CurrentConnection => currentConnection;

		public async Task SetProviderAsync(IDbProvider dbProvider, Connection connection, Action saveConnections) {
			Provider = dbProvider;
			currentConnection = connection;
			saveConnectionsAction = saveConnections;
			Capabilities = dbCapabilities.For(dbProvider);

			await ReloadDatabasesAsync();
			LoadLastSelectedDatabase();
		}

		private List<DbInfo> databases;
		public List<DbInfo> Databases {
			get => databases;
			set => this.RaiseAndSetIfChanged(ref databases, value);
		}

		private DbInfo selectedDatabase;
		public DbInfo SelectedDatabase {
			get => selectedDatabase;
			set => this.RaiseAndSetIfChanged(ref selectedDatabase, value);
		}

		public bool ShouldCloseLauncherAfterStart { get; set; } = true;

		private readonly LauncherOptions launcherOptions;

		/// <summary>
		/// Указывает, должна ли быть видна галочка "Не закрывать лаунчер после подключения".
		/// Видна только в standalone режиме (когда лаунчер - отдельное приложение).
		/// </summary>
		public bool VisibleShouldCloseLauncherCheckBox => launcherOptions?.IsStandalone ?? false;

		public ICommand ConnectCommand { get; }
		public ReactiveCommand<Unit, Unit> OpenCreateDatabaseCommand { get; }
		public ReactiveCommand<Unit, Unit> OpenImportDatabaseCommand { get; }
		public ICommand BackupDatabaseCommand { get; }
		public ICommand DeleteDatabaseCommand { get; }
		public ReactiveCommand<Unit, Unit> OpenUserManagementCommand { get; }
		public ReactiveCommand<Unit, Unit> OpenChangePasswordCommand { get; }
		public ReactiveCommand<Unit, Unit> RefreshMetadataCommand { get; }

		public ReactiveCommand<Unit, Unit> RefreshDatabasesCommand { get; }

		/// <summary>«Назад» - возврат к выбору подключения</summary>

		private readonly IInteractiveMessage interactiveMessage;
		private readonly IInteractiveQuestion interactiveQuestion;
		private readonly IServiceProvider serviceProvider;

		private readonly IAppRunner appRunner;
		private readonly DbCapabilities dbCapabilities;
		private readonly IErrorHandlingService errorHandling;

		public DataBasesVM(
			IAppRunner appRunner,
			IInteractiveMessage interactiveMessage,
			IInteractiveQuestion interactiveQuestion,
			LauncherOptions launcherOptions,
			IServiceProvider serviceProvider,
			DbCapabilities capabilities,
			IErrorHandlingService errorHandling) : base(navigation)
		{
			this.errorHandling = errorHandling ?? throw new ArgumentNullException(nameof(errorHandling));
			Launch = launch ?? throw new ArgumentNullException(nameof(launch));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.interactiveQuestion = interactiveQuestion ?? throw new ArgumentNullException(nameof(interactiveQuestion));
			this.launcherOptions = launcherOptions;
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.dbCapabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));

			// запуск приложения закрывает лаунчер, а с ним и фоновую работу - поэтому
			// «Запустить» держим закрытым, пока страница занята, иначе Shutdown обрывает
			// операцию на середине
			IObservable<bool> canExecuteConnection = this
				.WhenAnyValue(x => x.SelectedDatabase, x => x.IsBusy,
					(database, busy) => database != null && !busy);

			ConnectCommand = ReactiveCommand.CreateFromTask(
				() => RunBusyAsync("Подключение к базе данных", ConnectAsync), canExecuteConnection);
			OpenCreateDatabaseCommand = ReactiveCommand.Create(OpenCreateDatabase);
			OpenImportDatabaseCommand = ReactiveCommand.Create(OpenImportDatabase);
			BackupDatabaseCommand = ReactiveCommand.Create<DbInfo>(OpenBackup);
			DeleteDatabaseCommand = ReactiveCommand.CreateFromTask<DbInfo>(DeleteDatabaseAsync);
			OpenUserManagementCommand = ReactiveCommand.Create(OpenUsers);
			OpenChangePasswordCommand = ReactiveCommand.Create(ChangePassword);
			RefreshMetadataCommand = ReactiveCommand.Create(OpenRefreshMetadata);
			RefreshDatabasesCommand = ReactiveCommand.CreateFromTask(
				() => RunBusyAsync("Обновление списка баз", RefreshDatabases));
			BackCommand = ReactiveCommand.Create(Navigation.Previous);
		}

		private void OpenRefreshMetadata() {
			if(Provider == null || !Capabilities.CanRefreshMetadata)
				return;

			int syncedBases = 0;
			int syncedUsers = 0;

			var phases = new[] {
				new DbCreationPhase("Синхронизация баз данных", args => {
					var response = args.Provider.RefreshBases();
					syncedBases = response.SyncedBases;
					args.FailureReason = response.ErrorMessage;
					return response.Success;
				}),
				new DbCreationPhase("Синхронизация пользователей", args => {
					var response = args.Provider.RefreshUsers();
					syncedUsers = response.SyncedUsers;
					args.FailureReason = response.ErrorMessage;
					return response.Success;
				})
			};

			var progress = serviceProvider.GetRequiredService<CreateDataBaseProgressVM>();
			progress.OperationTitle = RefreshMetadataTitle;
			progress.SetPipeline(Provider, currentConnection, phases);
			progress.OperationCompleted += () => CompleteRefreshMetadata(syncedBases, syncedUsers);
			// причину отказа страница прогресса показывает сама, нам остаётся только увести
			// пользователя обратно, когда он её прочитал и закрыл
			progress.CloseRequested += () => ReturnToDatabases(refreshList: syncedBases > 0);
			Navigation.Push(progress);
		}

		private void CompleteRefreshMetadata(int syncedBases, int syncedUsers) {
			ReturnToDatabases();

			interactiveMessage.ShowMessage(ImportanceLevel.Success,
				$"Метаинформация обновлена.\nБаз: {syncedBases}, пользователей: {syncedUsers}.",
				RefreshMetadataTitle);
		}

		private void ChangePassword() {
			if(Provider == null)
				return;

			var vm = serviceProvider.GetRequiredService<ChangePasswordVM>();
			vm.SetProvider(Provider);
			Navigation.Push(vm);
		}

		private void OpenUsers() {
			if(Provider == null)
				return;

			var vm = serviceProvider.GetRequiredService<UsersVM>();
			vm.SetProvider(Provider);
			Navigation.Push(vm);
		}

		/// <summary>
		/// открывает страницу создания базы; по завершении возвращает фокус на <see cref="DataBasesVM"/> и обновляет список баз
		/// </summary>
		private void OpenCreateDatabase() {
			if(!CanCreateDatabase)
				return;

			var settings = new CreateDbSettingsVM(Provider, CurrentConnection, serviceProvider, interactiveMessage);
			settings.OperationCompleted += () => OnOperationCompleted(settings);
			PushPageCommand?.Execute(settings);
		}

		/// <summary>
		/// открывает страницу импорта базы из дампа; по завершении возвращает фокус на <see cref="DataBasesVM"/> и обновляет список баз
		/// </summary>
		private void OpenImportDatabase() {
			if(!CanImportDatabase)
				return;

			var settings = new ImportDbSettingsVM(Provider, CurrentConnection, serviceProvider, interactiveMessage);
			settings.OperationCompleted += () => OnOperationCompleted(settings);
			PushPageCommand?.Execute(settings);
		}

		/// <summary>
		/// открывает страницу резервного копирования выбранной базы
		/// </summary>
		private void OpenBackup(DbInfo database) {
			if(database == null || !CanBackupDatabase)
				return;

			var settings = new BackupDbSettingsVM(database, Provider, CurrentConnection, serviceProvider, interactiveMessage);
			settings.OperationCompleted += () => OnOperationCompleted(settings);
			PushPageCommand?.Execute(settings);
		}

		private void ReturnToDatabases(bool refreshList = true) {
			PopToPageCommand?.Execute(GetType());
			if(refreshList)
				RefreshDatabasesCommand.Execute().Subscribe();
		}

		private void OnOperationCompleted(DbOperationSettingsVM operation) {
			ReturnToDatabases();

			if(operation is BackupDbSettingsVM backup)
				interactiveMessage.ShowMessage(ImportanceLevel.Success,
					$"Резервная копия базы данных сохранена:\n{backup.BackupFilePath}",
					"Резервное копирование");
		}

		private async Task DeleteDatabaseAsync(DbInfo database) {
			if(database == null || !Capabilities.CanDrop)
				return;

			// вопрос задаём до того, как перекрыли страницу: перекрытие показывает ход
			// операции, а операция начинается только с согласия пользователя
			bool confirmed = await interactiveQuestion.AskInBackground(
				$"Безвозвратно удалить базу данных «{database.Title}»?", DropDatabaseTitle);
			if(!confirmed)
				return;

			await RunBusyAsync(DropDatabaseTitle, async () => {
				try {
					await Task.Run(() => Provider.DropDatabase(database));
					await RefreshDatabases();
					interactiveMessage.ShowMessage(ImportanceLevel.Success,
						$"База данных {database.Title} удалена.", DropDatabaseTitle);
				}
				catch(Exception ex) {
					errorHandling.Handle(ex, DropDatabaseTitle);
				}
			});
		}

		public async Task RefreshDatabases() {
			if(Provider == null)
				return;

			int? selectedBaseId = SelectedDatabase?.BaseId;
			await ReloadDatabasesAsync();

			// список пересобран, прежний объект в нём уже другой - ищем по идентификатору.
			// Иначе после каждой операции выбор прыгает на первую строку
			SelectedDatabase = Databases.FirstOrDefault(db => db.BaseId == selectedBaseId)
				?? Databases.FirstOrDefault();
		}

		/// <summary>
		/// Слой доступа к базе синхронный и блокирующий, поэтому чтение уводим в фон;
		/// список и уведомление обновляем уже после await, на потоке интерфейса
		/// </summary>
		private async Task ReloadDatabasesAsync() {
			Databases = await Task.Run(() => Provider.GetUserDatabases().AsList());
			this.RaisePropertyChanged(nameof(Databases));
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

		/// <summary>Синхронный вход для Gtk-представления, у которого обработчики событий не асинхронны</summary>
		public void Connect() => ConnectCommand.Execute(null);

		public async Task ConnectAsync() {
			// вход в базу это поход на сервер, а в облаке ещё и запуск сессии - уводим в фон,
			// иначе интерфейс стоит всё время подключения
			var resp = await Task.Run(() => Provider.LoginToDatabase(SelectedDatabase));
			if(!resp.Success) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Ошибка подключения к базе данных");
				return;
			}

			SaveLastSelectedDatabase();

			// Определяем, нужно ли закрывать лаунчер через Shutdown
			// В standalone режиме учитываем галочку ShouldCloseLauncherAfterStart
			// В in-process режиме НЕ делаем shutdown (возвращаем false)
			var isStandalone = launcherOptions?.IsStandalone ?? false;
			bool shouldCloseLauncher = isStandalone && ShouldCloseLauncherAfterStart;

			logger.Debug("Запуск приложения: standalone {0}, закрывать лаунчер {1}", isStandalone, shouldCloseLauncher);

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
