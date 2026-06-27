using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public abstract class DbOperationSettingsVM : CarouselPageVM {
		protected IDbProvider Provider { get; }
		protected Connection Connection { get; }
		protected IServiceProvider Services { get; }

		protected DbOperationSettingsVM(IDbProvider provider, Connection connection, IServiceProvider services) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			Services = services ?? throw new ArgumentNullException(nameof(services));

			CancelCommand = ReactiveCommand.Create(() => PopPageCommand?.Execute(null));
		}

		public abstract string Title { get; }

		public IObservable<bool> CanProceed { get; private set; }

		public abstract IEnumerable<DbCreationPhase> BuildPipeline();

		public ReactiveCommand<Unit, Unit> ProceedCommand { get; private set; }
		public ReactiveCommand<Unit, Unit> CancelCommand { get; }
		public event Action OperationCompleted;

		protected void SetValidity(IObservable<bool> canProceed) {
			CanProceed = canProceed ?? Observable.Return(true);
			ProceedCommand = ReactiveCommand.Create(GoToProgress, CanProceed);
		}

		private void GoToProgress() {
			var progress = ActivatorUtilities.GetServiceOrCreateInstance<CreateDataBaseProgressVM>(Services);
			progress.OperationTitle = Title;
			progress.SetPipeline(Provider, Connection, BuildPipeline());
			progress.OperationCompleted += () => OperationCompleted?.Invoke();
			PushPageCommand?.Execute(progress);
		}
	}
}
