using ReactiveUI;
using System;
using System.Windows.Input;

namespace QS.Launcher.ViewModels.Commands;

public class ChangePageCommand(MainWindowVM vm) : ICommand {
	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) {
		return true;
	}

	public void Execute(object? parameter) {
		vm.ChangePage(parameter is int v ? v : 0);
	}
}

public class NextPageCommand(MainWindowVM vm) : ICommand {
	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) {
		return true;
	}

	public void Execute(object? parameter) {
		vm.NextPage();
	}
}

public class PreviousPageCommand(MainWindowVM vm) : ICommand {
	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) {
		return true;
	}

	public void Execute(object? parameter) {
		vm.PreviousPage();
	}
}
