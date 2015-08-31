using System;
using Gtk;
using Gamma.Binding.Core;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yLabel : Label
	{
		public BindingControler<yLabel> Binding { get; private set;}

		public yLabel ()
		{
			Binding = new BindingControler<yLabel> (this);
		}
	}
}

