using DynamicData.Kernel;
using QS.DbManagement;
using QS.Dialog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using QS.Launcher.AppRunner;
using QS.Project.Versioning;

namespace QS.Launcher.ViewModels.PageViewModels {
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
				
				// Загружаем и устанавливаем последнюю выбранную базу
				LoadLastSelectedDatabase();
			}
		}
		
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

		public event Action<bool> StartLaunchProgram;

		IInteractiveMessage interactiveMessage;

		private readonly IAppRunner appRunner;
		private readonly IApplicationInfo applicationInfo;

		public DataBasesVM(IAppRunner appRunner, IApplicationInfo applicationInfo, IInteractiveMessage interactiveMessage, LauncherOptions launcherOptions) {
			this.appRunner = appRunner ?? throw new ArgumentNullException(nameof(appRunner));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			this.launcherOptions = launcherOptions;
			
			logger.Info($">>> DataBasesVM constructor: launcherOptions={launcherOptions}, IsStandalone={launcherOptions?.IsStandalone}");

			IObservable<bool> canExecute = this
				.WhenAnyValue(x => x.SelectedDatabase)
				.Select(x => x != null);
			ConnectCommand = ReactiveCommand.Create(Connect, canExecute);
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

			// Сохраняем последнюю выбранную базу
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
			
			// Сохраняем BaseId в текущее подключение
			currentConnection.LastBaseId = SelectedDatabase.BaseId;
			
			// Вызываем сохранение подключений
			saveConnectionsAction?.Invoke();
		}
	}
}
