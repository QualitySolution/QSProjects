using System;
using Gtk;
using Gamma.Binding.Core;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yImage : Image
	{
		public BindingControler<yImage> Binding { get; private set;}

		public yImage()
		{
			Binding = new BindingControler<yImage> (this);
		}
	}
}

