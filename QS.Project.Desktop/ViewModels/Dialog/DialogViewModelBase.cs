using System;
using System.Threading;
using QS.Navigation;
using QS.ViewModels.Extension;

namespace QS.ViewModels.Dialog
{
	/// <summary>
	/// Базовый класс для всех ViewModel представляющих собой диалоги. Это либо вкладки, либо окна в интерфейсе. 
	/// Которые пользователь открывает для совершения каких-то отдельных действий.
	/// Внутри кода, такие диалоги открываются через INavigationManager
	/// </summary>
	public abstract class DialogViewModelBase : ViewModelBase, IDialogViewModel, IBusyViewModel
	{
		public INavigationManager NavigationManager { get; set; }

		protected DialogViewModelBase(INavigationManager navigation)
		{
			//FIXME Когда выпилим ViewModel с TDI, добавить проверку на null;
			this.NavigationManager = navigation;
		}

		private string title;
		public virtual string Title {
			get => title;
			set => SetField(ref title, value);
		}

		#region Длительные операции

		private BusyOperation activeBusyOperation;
		private string busyOperationTitle;

		public bool IsBusy => activeBusyOperation != null;
		public bool CanCancelBusyOperation => activeBusyOperation?.CanCancel == true
			&& !activeBusyOperation.CancellationToken.IsCancellationRequested;
		public bool IsBusyCancellationRequested => activeBusyOperation?.CancellationToken.IsCancellationRequested == true;
		public string BusyOperationTitle => busyOperationTitle;
		public CancellationToken BusyOperationToken => activeBusyOperation?.CancellationToken ?? CancellationToken.None;

		public virtual IBusyOperation BeginBusyOperation(string title = null, bool canCancel = false)
		{
			if(IsBusy)
				throw new InvalidOperationException($"ViewModel «{Title}» уже выполняет длительную операцию.");

			busyOperationTitle = title;
			activeBusyOperation = new BusyOperation(this, canCancel);
			OnPropertyChanged(nameof(IsBusy));
			OnPropertyChanged(nameof(CanCancelBusyOperation));
			OnPropertyChanged(nameof(IsBusyCancellationRequested));
			OnPropertyChanged(nameof(BusyOperationTitle));
			OnPropertyChanged(nameof(BusyOperationToken));
			return activeBusyOperation;
		}

		public virtual bool RequestCancelBusyOperation()
		{
			if(!CanCancelBusyOperation)
				return false;

			activeBusyOperation.Cancel();
			OnPropertyChanged(nameof(CanCancelBusyOperation));
			OnPropertyChanged(nameof(IsBusyCancellationRequested));
			return true;
		}

		private void EndBusyOperation(BusyOperation operation)
		{
			if(!ReferenceEquals(activeBusyOperation, operation))
				return;

			activeBusyOperation = null;
			busyOperationTitle = null;
			OnPropertyChanged(nameof(IsBusy));
			OnPropertyChanged(nameof(CanCancelBusyOperation));
			OnPropertyChanged(nameof(IsBusyCancellationRequested));
			OnPropertyChanged(nameof(BusyOperationTitle));
			OnPropertyChanged(nameof(BusyOperationToken));
		}

		private sealed class BusyOperation : IBusyOperation
		{
			private DialogViewModelBase owner;
			private readonly CancellationTokenSource cancellationTokenSource;

			public BusyOperation(DialogViewModelBase owner, bool canCancel)
			{
				this.owner = owner;
				CanCancel = canCancel;
				cancellationTokenSource = canCancel ? new CancellationTokenSource() : null;
				CancellationToken = cancellationTokenSource?.Token ?? System.Threading.CancellationToken.None;
			}

			public bool CanCancel { get; }
			public CancellationToken CancellationToken { get; }

			public void Cancel() => cancellationTokenSource?.Cancel();

			public void Dispose()
			{
				var currentOwner = Interlocked.Exchange(ref owner, null);
				if(currentOwner == null)
					return;

				currentOwner.EndBusyOperation(this);
				cancellationTokenSource?.Dispose();
			}
		}

		#endregion

		public virtual void Close(bool askClose, CloseSource source)
		{
			var page = NavigationManager?.FindPage(this);
			if(page != null) {
				if(askClose)
					NavigationManager.AskClosePage(page, source);
				else
					NavigationManager.ForceClosePage(page, source);
			}
		}
	}
}
