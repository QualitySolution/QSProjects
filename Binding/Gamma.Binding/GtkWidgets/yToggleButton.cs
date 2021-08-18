using System;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yToggleButton : ToggleButton
	{
		public BindingControler<yToggleButton> Binding { get; private set; }

		public yToggleButton()
		{
			Binding = new BindingControler<yToggleButton>(this);
		}
	}
}
