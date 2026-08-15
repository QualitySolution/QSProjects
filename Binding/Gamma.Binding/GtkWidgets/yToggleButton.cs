using Gamma.Binding.Core;
using Gtk;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yToggleButton : ToggleButton
	{
		private ICommand command;
		private Func<object> commandArgument;

		public BindingControler<yToggleButton> Binding { get; private set; }

		public yToggleButton()
		{
			Binding = new BindingControler<yToggleButton>(this, new Expression<Func<yToggleButton, object>>[] {
				(w => w.Active)
			});
		}

		protected override void OnToggled() {
			Binding.FireChange(w => w.Active);
			base.OnToggled();
		}

		public void BindCommand(ICommand command, Func<object> commandArgument = null) {
			if(this.command != null) {
				throw new InvalidOperationException("Биндинг можно настроить только для одной команды");
			}

			this.command = command;
			this.commandArgument = commandArgument;
			command.CanExecuteChanged += CommandCanExecuteChanged;
			Sensitive = command.CanExecute(commandArgument);

			if(command is BusyCommand busyCommand) {
				Label = busyCommand.Text;
				busyCommand.PropertyChanged += BusyCommandPropertyChanged;
			}
		}

		private void BusyCommandPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if(e.PropertyName == nameof(BusyCommand.Text)) {
				Label = (sender as BusyCommand).Text;
				while(Application.EventsPending()) {
					Gtk.Main.Iteration();
				}
			}
		}

		protected override void OnClicked() {
			base.OnClicked();

			if(command != null) {
				command.Execute(commandArgument);
			}
		}

		private void CommandCanExecuteChanged(object sender, EventArgs e) {
			Sensitive = command.CanExecute(commandArgument);
		}

		protected override void OnDestroyed() {
			if(command != null) {
				command.CanExecuteChanged -= CommandCanExecuteChanged;

				if(command is BusyCommand busyCommand) {
					busyCommand.PropertyChanged -= BusyCommandPropertyChanged;
				}
			}
			Binding.CleanSources();
			base.OnDestroyed();
		}
	}
}
