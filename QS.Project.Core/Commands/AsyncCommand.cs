using System;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Commands {
	public class AsyncCommand : PropertySubscribedCommandBase {
		private CancellationTokenSource _cancelationTokenSource;
		private readonly Func<CancellationToken, Task> _handler;
		private readonly Func<bool> _canExecute;
		private Task _runningTask;

		public AsyncCommand(Func<CancellationToken, Task> handler, Func<bool> canExecute) {
			_cancelationTokenSource = new CancellationTokenSource();
			_handler = handler;
			_canExecute = canExecute;
		}

		public AsyncCommand(Func<CancellationToken, Task> handler) : this(handler, () => true) {
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
				_runningTask = Task.Run(
					async () => {
						RaiseCanExecuteChanged();
						try {
							await _handler(_cancelationTokenSource.Token);
						}
						finally {
							_runningTask = null;
							_cancelationTokenSource = new CancellationTokenSource();
							RaiseCanExecuteChanged();
						}
					},
					_cancelationTokenSource.Token);
			}
		}

		public void Abort() {
			_cancelationTokenSource.Cancel();
		}
	}
}
