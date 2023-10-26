using Gtk;
using Gamma.Binding.Core;
using System.Windows.Input;
using System;

namespace Gamma.GtkWidgets {
	[System.ComponentModel.ToolboxItem(false)]
	[System.ComponentModel.Category("Gamma Gtk")]

	public class yMenuItem : MenuItem 
	{

		private ICommand command;
		private Func<object> commandArgument;

		public BindingControler<yMenuItem> Binding { get; private set;}
		
		public yMenuItem() {
			Binding = new BindingControler<yMenuItem> (this);
		}

		public yMenuItem(string label) : base(label) {
			Binding = new BindingControler<yMenuItem> (this);
		}

		public void BindCommand(ICommand command, Func<object> commandArgument = null) {
			if(this.command != null) {
				throw new InvalidOperationException("Биндинг можно настроить только для одной команды");
			}

			this.command = command;
			this.commandArgument = commandArgument;
			command.CanExecuteChanged += CommandCanExecuteChanged;
		}

		protected override void OnActivated() {
			base.OnActivated();

			if(command != null) {
				command.Execute(commandArgument);
			}
		}

		private void CommandCanExecuteChanged(object sender, EventArgs e) {
			Sensitive = command.CanExecute(commandArgument);
		}

		public override void Destroy() {
			command.CanExecuteChanged -= CommandCanExecuteChanged;
			base.Destroy();
		}
	}
}
