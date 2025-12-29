using System;
using Gtk;
using Gamma.Binding.Core;
using System.Windows.Input;

namespace Gamma.GtkWidgets {
	[System.ComponentModel.ToolboxItem(false)]
	[System.ComponentModel.Category("Gamma Gtk")]

	public class yMenuItem : MenuItem 
	{
		private ICommand _command;
		private object _commandArgument;

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
			if(_command != null) {
				throw new InvalidOperationException("Биндинг можно настроить только для одной команды");
			}

			_command = command;
			_commandArgument = commandArgument;
			command.CanExecuteChanged += CommandCanExecuteChanged;
			Sensitive = command.CanExecute(commandArgument);
		}

		protected override void OnActivated() {
			base.OnActivated();
			_command?.Execute(_commandArgument);
		}

		private void CommandCanExecuteChanged(object sender, EventArgs e) {
			Sensitive = _command.CanExecute(_commandArgument);
		}

		protected override void OnDestroyed() {
			if(_command != null) {
				_command.CanExecuteChanged -= CommandCanExecuteChanged;
				_command = null;
			}
			
			base.OnDestroyed();
		}
	}
}
