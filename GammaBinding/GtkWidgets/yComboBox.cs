using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yComboBox : ComboBox
	{
		public BindingControler<yComboBox> Binding { get; private set;}

		public yComboBox ()
		{
			Binding = new BindingControler<yComboBox> (this, new Expression<Func<yComboBox, object>>[] {
				(w => w.Active),
				(w => w.ActiveText),
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Active, w => w.ActiveText);
			base.OnChanged ();
		}
	}
}

