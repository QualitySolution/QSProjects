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

		public new string ActiveText
		{
			get{ return base.ActiveText;}
			set{
				TreeIter iter;
				if(Gamma.GtkHelpers.ListStoreHelper.SearchListStore ((ListStore)Model, value, 0, out iter))
					SetActiveIter (iter);
			}
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Active, w => w.ActiveText);
			base.OnChanged ();
		}
	}
}

