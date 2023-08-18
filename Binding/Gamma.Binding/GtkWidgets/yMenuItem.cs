using System;
using Gtk;
using Gamma.Binding.Core;

namespace Gamma.GtkWidgets {
	[System.ComponentModel.ToolboxItem(false)]
	[System.ComponentModel.Category("Gamma Gtk")]

	public class yMenuItem : MenuItem {
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
	}
}
