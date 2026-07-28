using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Gamma.Binding.Core {
	public class BusyCommand : ICommand, INotifyPropertyChanged {
		private readonly Action _execute;
		private readonly Func<bool> _canExecute;

		private bool _isBusy;
		private string _text;
		private readonly string _normalText;
		private readonly string _busyText;

		public BusyCommand(
			string text,
			Action execute,
			Func<bool> canExecute = null,
			string busyText = "Выполняется..."
			) 
		{
			_execute = execute;
			_canExecute = canExecute;

			_text = text;
			_normalText = text;
			_busyText = busyText;
		}

		public string Text {
			get => _text;
			private set {
				if(_text == value)
					return;

				_text = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
			}
		}

		public bool IsBusy {
			get => _isBusy;
			private set {
				if(_isBusy == value) {
					return;
				}

				_isBusy = value;

				RaiseCanExecuteChanged();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
				Text = value ? _busyText : _normalText;
			}
		}

		public bool CanExecute(object parameter) {
			return !IsBusy && (_canExecute?.Invoke() ?? true);
		}

		public void Execute(object parameter) {
			if(!CanExecute(parameter))
				return;

			IsBusy = true;

			try {
				_execute.Invoke();
			}
			finally {
				IsBusy = false;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler CanExecuteChanged;
		public void RaiseCanExecuteChanged() {
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
