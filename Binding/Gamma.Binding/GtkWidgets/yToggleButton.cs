using System;
using System.Linq.Expressions;
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
			Binding = new BindingControler<yToggleButton>(this, new Expression<Func<yToggleButton, object>>[] {
				(w => w.Active)
			});
		}

		protected override void OnToggled() {
			Binding.FireChange(w => w.Active);
			base.OnToggled();
		}
	}
}
