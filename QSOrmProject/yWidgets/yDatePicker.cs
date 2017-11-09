using System;
using QSWidgetLib;
using System.ComponentModel;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
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

