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
	}
}
