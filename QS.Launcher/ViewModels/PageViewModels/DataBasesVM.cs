using DynamicData.Kernel;
using QS.DbManagement;
using QS.Dialog;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using QS.Launcher.AppRunner;
using QS.Project.Versioning;

namespace QS.Launcher.ViewModels.PageViewModels {
	public class DataBasesVM : CarouselPageVM {

		private IDbProvider provider;
		public IDbProvider Provider {
			get => provider;
			set {
				this.RaiseAndSetIfChanged(ref provider, value);
				Databases = provider.GetUserDatabases(applicationInfo).AsList();
				this.RaisePropertyChanged(nameof(Databases));
			}
		}

		public List<DbInfo> Databases { get; set; }

		private DbInfo selectedDatabase;
		public DbInfo SelectedDatabase {
			get => selectedDatabase;
			set => this.RaiseAndSetIfChanged(ref selectedDatabase, value);
		}

		public bool IsAdmin { get; set; } = false;

		public bool ShouldCloseLauncherAfterStart { get; set; } = true;

		public ICommand ConnectCommand { get; }

		public event Action StartClosingLauncherEvent;

		IInteractiveMessage interactiveMessage;

		private LauncherOptions launcherOptions;
		private readonly IAppRunner appRunner;
		private readonly IApplicationInfo applicationInfo;

		public DataBasesVM(LauncherOptions options, IAppRunner appRunner, IApplicationInfo applicationInfo, IInteractiveMessage interactiveMessage) {
			launcherOptions = options ?? throw new ArgumentNullException(nameof(options));
			this.appRunner = appRunner ?? throw new ArgumentNullException(nameof(appRunner));
			this.applicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));

			IObservable<bool> canExecute = this
				.WhenAnyValue(x => x.SelectedDatabase)
				.Select(x => x != null);
			ConnectCommand = ReactiveCommand.Create(Connect, canExecute);
		}

		public void Connect() {

			var resp = provider.LoginToDatabase(SelectedDatabase);

			if(!resp.Success) {
				interactiveMessage.ShowMessage(ImportanceLevel.Error, resp.ErrorMessage, "Ошибка подключения к базе данных");
				return;
			}

			appRunner.Run(resp);

			if(ShouldCloseLauncherAfterStart)
				StartClosingLauncherEvent?.Invoke();
		}
	}
}
