using System;
using System.Threading;
using System.Threading.Tasks;
using QS.Dialog;

namespace QS.Commands {
	public class AsyncCommand : PropertySubscribedCommandBase {
		private CancellationTokenSource _cancelationTokenSource;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly Func<CancellationToken, Task> _handler;
		private readonly Func<bool> _canExecute;
		private Task _runningTask;

		public AsyncCommand(IGuiDispatcher guiDispatcher, Func<CancellationToken, Task> handler, Func<bool> canExecute) {
			_cancelationTokenSource = new CancellationTokenSource();
			_guiDispatcher = guiDispatcher;
			_handler = handler;
			_canExecute = canExecute;
		}

		public AsyncCommand(IGuiDispatcher guiDispatcher, Func<CancellationToken, Task> handler) : this(guiDispatcher, handler, () => true) {
		}

		public override bool CanExecute(object parameter) {
			return CanExecute();
		}

		public bool CanExecute() {
			if(_canExecute is null) {
				return false;
			}

			if(_cancelationTokenSource.IsCancellationRequested
				|| (_runningTask != null)) {
				return false;
			}

			return _canExecute.Invoke();
		}

		public override void Execute(object parameter) {
			Execute();
		}

		public void Execute() {
			if(CanExecute()) {
				_runningTask = new Task(async () => {
					try {
						await _handler(_cancelationTokenSource.Token);
					}
					finally {
						_runningTask = null;
						_cancelationTokenSource.Cancel();
						_cancelationTokenSource.Dispose();
						_cancelationTokenSource = new CancellationTokenSource();
						_guiDispatcher.RunInGuiTread(() => {
							RaiseCanExecuteChanged();
						});
					}
				}, _cancelationTokenSource.Token);
				
				RaiseCanExecuteChanged();

				_runningTask.Start();
			}
		}

		public void Abort() {
			_cancelationTokenSource.Cancel();
		}
	}
}
