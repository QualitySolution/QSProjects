using System;
using System.Reactive;
using System.Reactive.Linq;
using QS.DbManagement;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	/// <summary>
	/// По «Далее» создаёт CreateDataBaseProgressVM и пушит её в Carousel поверх текущей
	/// </summary>
	public class CreateDataBaseSettingsVM : CarouselPageVM {
		public IDbProvider Provider { get; }
		public Connection Connection { get; }
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

		public CreateDataBaseSettingsVM(IDbProvider provider, Connection connection, IServiceProvider services) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.services = services ?? throw new ArgumentNullException(nameof(services));

			var canCreate = this.WhenAnyValue(x => x.DbName, x => x.DbTitle,
				(name, title) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(title));

			CreateDataBaseCommand = ReactiveCommand.Create(GoToProgress, canCreate);
			CancelCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));
		}

		private void GoToProgress() {
			// Резолв через ActivatorUtilities — DI подставляет IDbCreatorInteraction/IUiThreadInvoker,
			// а провайдер/соединение/имена приходят как runtime-аргументы.
			var progress = Microsoft.Extensions.DependencyInjection.ActivatorUtilities
				.CreateInstance<CreateDataBaseProgressVM>(services, Provider, Connection, DbName, DbTitle);

			ProgressPageRequested?.Invoke(progress);
			PushPageCommand?.Execute(progress);
		}
	}
}
