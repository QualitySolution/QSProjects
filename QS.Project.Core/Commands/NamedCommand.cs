using System;
namespace QS.Commands
{
	public class NamedCommand : DelegateCommand {

		public NamedCommand(string name, Action execute) : base(execute) {
			Name = name;
		}

		public NamedCommand(string name, Action execute, Func<bool> canExecute) : base(execute, canExecute) {
			Name = name;
		}
		public string Name { get; }
	}
}
