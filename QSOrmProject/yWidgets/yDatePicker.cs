using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSWidgetLib;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	[Obsolete("Используйте новый виджет QS.Widgets.GtkUI.DatePicker")]
	public class yDatePicker : DatePicker
	{
		public BindingControler<yDatePicker> Binding { get; private set;}

		public yDatePicker ()
		{
			Binding = new BindingControler<yDatePicker> (this, new Expression<Func<yDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
		}

		protected override void OnDateChanged ()
		{
			Binding.FireChange (new Expression<Func<yDatePicker, object>>[] {
				(w => w.Date),
				(w => w.DateOrNull),
				(w => w.DateText),
				(w => w.IsEmpty)
			});
			base.OnDateChanged ();
		}
	}
}

