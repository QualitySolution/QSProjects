using System;
namespace QS.Commands
{
	public class DelegateCommand : PropertySubscribedCommandBase
	{
		private readonly Func<bool> canExecute;
		private readonly Action execute;

		public DelegateCommand(Action execute, Func<bool> canExecute)
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public DelegateCommand(Action execute) : this(execute, () => true)
		{
		}

		public bool CanExecute()
		{
            if(canExecute == null)
            {
				return false;
            }
			return canExecute.Invoke();
		}

		public override bool CanExecute(object parameter)
		{
			return CanExecute();
		}

		public void Execute()
		{
			if(CanExecute()) {
				execute.Invoke();
			}
		}

		public override void Execute(object parameter)
		{
			Execute();
		}
	}
}
