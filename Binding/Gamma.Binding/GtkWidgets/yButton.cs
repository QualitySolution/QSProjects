using System;
using System.Windows.Input;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yButton : Button
	{
		private ICommand command;
		private Func<object> commandArgument;

		public BindingControler<yButton> Binding { get; private set; }

		public yButton()
		{
			Binding = new BindingControler<yButton>(this);
			Clicked += (sender, args) => GrabFocus();
		}

		public void BindCommand(ICommand command, Func<object> commandArgument = null)
		{
			if(this.command != null) {
				throw new InvalidOperationException("Биндинг можно настроить только для одной команды");
			}

			this.command = command;
			this.commandArgument = commandArgument;
			command.CanExecuteChanged += CommandCanExecuteChanged;
			Sensitive = command.CanExecute(commandArgument);
		}

		protected override void OnClicked() 
		{
			base.OnClicked();

			if(command != null) {
				command.Execute(commandArgument);
			}
		}

		private void CommandCanExecuteChanged(object sender, EventArgs e) 
		{
			Sensitive = command.CanExecute(commandArgument);
		}

		protected override void OnDestroyed() {
			if(command != null) {
				command.CanExecuteChanged -= CommandCanExecuteChanged;
			}
			Binding.CleanSources();
			base.OnDestroyed();
		}
	}
}
