using Gamma.Binding.Core;
using Gtk;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Gamma.GtkWidgets {
	[System.ComponentModel.ToolboxItem(false)]
	[System.ComponentModel.Category("Gamma Gtk")]

	public class yMenuItem : MenuItem 
	{
		private ICommand command;
		private object commandArgument;

		public BindingControler<yMenuItem> Binding { get; private set;}
		
		public yMenuItem() {
			Binding = new BindingControler<yMenuItem> (this);
		}

		public yMenuItem(string label) : base(label) {
			Binding = new BindingControler<yMenuItem> (this);
		}

		public string Label {
			get => ((Label)Child)?.LabelProp;
			set {
				if(Child is Label label)
					label.LabelProp = value;
				else
					throw new InvalidOperationException("Данное свойство можно использовать только если дочерний виджет является Label. Возможно использован пустой конструктор либо задан другой виджет");
				((Label)Child).LabelProp = value;
			}
		}
		
		public void BindCommand(ICommand command, object commandArgument = null) {
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

		protected override void OnActivated() {
			base.OnActivated();
			command?.Execute(commandArgument);
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
				command = null;
			}
			
			base.OnDestroyed();
		}
	}
}
