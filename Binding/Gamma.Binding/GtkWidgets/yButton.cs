using System;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class yButton : Button
	{
		public BindingControler<yButton> Binding { get; private set; }

		public yButton()
		{
			Binding = new BindingControler<yButton>(this);
			Clicked += (sender, args) => GrabFocus();
		}

		protected override void OnDestroyed() {
			Binding.CleanSources();
			base.OnDestroyed();
		}
	}
}
