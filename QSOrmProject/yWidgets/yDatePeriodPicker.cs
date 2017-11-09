using System;
using QSWidgetLib;
using System.ComponentModel;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.Widgets
{
	[ToolboxItem (true)]
	[Category ("Gamma Widgets")]
	public class yDatePeriodPicker : DatePeriodPicker
	{
		public BindingControler<yDatePeriodPicker> Binding { get; private set;}

		public yDatePeriodPicker ()
		{
			Binding = new BindingControler<yDatePeriodPicker> (this, new Expression<Func<yDatePeriodPicker, object>>[] {
				(w => w.StartDate),
				(w => w.StartDateOrNull),
				(w => w.EndDate),
				(w => w.EndDateOrNull)
			});
		}

        protected override void OnStartDateChanged()
        {
			Binding.FireChange(new Expression<Func<yDatePeriodPicker, object>>[] {
				(w => w.StartDate),
				(w => w.StartDateOrNull),
			});

			base.OnStartDateChanged();
        }

        protected override void OnEndDateChanged()
        {
			Binding.FireChange(new Expression<Func<yDatePeriodPicker, object>>[] {
				(w => w.EndDate),
				(w => w.EndDateOrNull),
			});

			base.OnEndDateChanged();
        }
	}
}

