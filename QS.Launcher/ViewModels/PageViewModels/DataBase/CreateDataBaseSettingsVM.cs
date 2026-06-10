using System;
using System.Reactive;
using System.Reactive.Linq;
using QS.DbManagement;
using QS.DBScripts.Controllers;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public class CreateDataBaseSettingsVM : CarouselPageVM {
		public IDbProvider Provider { get; private set; }
		public Connection Connection { get; private set; }
		private readonly IServiceProvider services;

		private string dbTitle;
		public string DbTitle {
			get => dbTitle;
			set => this.RaiseAndSetIfChanged(ref dbTitle, value);
		}

		private string dbName;
		public string DbName {
			get => dbName;
			set => this.RaiseAndSetIfChanged(ref dbName, value);
		}

		public ReactiveCommand<Unit, Unit> CreateDataBaseCommand { get; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }

		/// <summary>
		/// Сообщает заинтересованным о том, что только что создана
		/// progress-VM и пора подписаться на её события
		/// </summary>
		public event Action<CreateDataBaseProgressVM> ProgressPageRequested;

		public CreateDataBaseSettingsVM(IServiceProvider services) {
			this.services = services ?? throw new ArgumentNullException(nameof(services));

			var canCreate = this.WhenAnyValue(x => x.DbName, x => x.DbTitle,
				(name, title) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title));

			CreateDataBaseCommand = ReactiveCommand.Create(GoToProgress, canCreate);
			CancelCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));
		}

		public void SetDbSettings(IDbProvider provider, Connection connection) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
		}

		private void GoToProgress() {
			var progress = Microsoft.Extensions.DependencyInjection.ActivatorUtilities
				.GetServiceOrCreateInstance<CreateDataBaseProgressVM>(services);

			var pipeline = new[] {
				new DbCreationPhase(
					"Создание базы данных",
					args => args.Provider.CreateDatabase(DbName, DbTitle, services)),
				new DbCreationPhase(
					"Наполнение базы данных",
					args => {
						IDbCreatorModel creator = Connection.ConnectionType.CreateCreator(args);
						return creator.RunCreation(DbName, DbTitle);
					})
			};

			progress.SetPipeline(Provider, Connection, pipeline);
			ProgressPageRequested?.Invoke(progress);
			PushPageCommand?.Execute(progress);
		}
	}
}
