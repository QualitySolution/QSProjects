using System;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yProgressBar : ProgressBar
	{
		public BindingControler<yProgressBar> Binding { get; private set; }

		public yProgressBar()
		{
			Binding = new BindingControler<yProgressBar>(this);
		}
	}
}
