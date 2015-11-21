using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class yEntry : Entry
	{
		public BindingControler<yEntry> Binding { get; private set;}

		public yEntry ()
		{
			Binding = new BindingControler<yEntry> (this, new Expression<Func<yEntry, object>>[] {
				(w => w.Text)
			});
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Text);
			base.OnChanged ();
		}
	}
}

