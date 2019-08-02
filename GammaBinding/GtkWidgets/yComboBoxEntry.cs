using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yComboBoxEntry : ComboBoxEntry
	{
		public BindingControler<yComboBoxEntry> Binding { get; private set;}

		public yComboBoxEntry()
		{
			Binding = new BindingControler<yComboBoxEntry> (this, new Expression<Func<yComboBoxEntry, object>>[] {
				(w => w.Active),
				(w => w.ActiveText),
				(w => w.Entry.Text)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Active, w => w.ActiveText, w => w.Entry.Text);
			base.OnChanged ();
		}
	}
}

