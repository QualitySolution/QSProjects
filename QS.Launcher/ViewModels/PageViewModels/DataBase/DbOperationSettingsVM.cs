using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using QS.DbManagement;
using QS.DbManagement.Entities;
using QS.Dialog;
using ReactiveUI;

namespace QS.Launcher.ViewModels.PageViewModels.DataBase {
	public abstract class DbOperationSettingsVM : CarouselPageVM {
		protected IDbManager Provider { get; }
		protected Connection Connection { get; }
		protected IServiceProvider Services { get; }

		private readonly IInteractiveMessage interactiveMessage;

		protected DbOperationSettingsVM(IDbManager provider, Connection connection, IServiceProvider services,
			IInteractiveMessage interactiveMessage) {
			Provider = provider ?? throw new ArgumentNullException(nameof(provider));
			Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			Services = services ?? throw new ArgumentNullException(nameof(services));
			this.interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));

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

		protected virtual string ValidationError() => null;

		private void GoToProgress() {
			string error = ValidationError();
			if(error != null) {
				interactiveMessage.ShowMessage(ImportanceLevel.Warning, error, Title);
				return;
			}

			var progress = Services.GetRequiredService<CreateDataBaseProgressVM>();
			progress.OperationTitle = Title;
			progress.SetPipeline(Provider, Connection, BuildPipeline());
			progress.OperationCompleted += () => OperationCompleted?.Invoke();
			progress.CloseRequested += () => PopPageCommand?.Execute(null);
			PushPageCommand?.Execute(progress);
		}
	}
}
