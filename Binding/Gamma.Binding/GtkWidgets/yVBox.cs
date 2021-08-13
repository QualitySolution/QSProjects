using System;
using Gtk;
using Gamma.Binding.Core;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yVBox : VBox
	{
		public BindingControler<yVBox> Binding { get; private set;}

		public yVBox()
		{
			Binding = new BindingControler<yVBox> (this);
		}
	}
}

