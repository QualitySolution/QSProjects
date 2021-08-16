using System;
using Gtk;
using Gamma.Binding.Core;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yHBox : HBox
	{
		public BindingControler<yHBox> Binding { get; private set;}

		public yHBox()
		{
			Binding = new BindingControler<yHBox> (this);
		}
	}
}

