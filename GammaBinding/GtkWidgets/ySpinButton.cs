using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace Gamma.GtkWidgets
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma Gtk")]
	public class ySpinButton : SpinButton
	{
		public BindingControler<ySpinButton> Binding { get; private set; }

		public ySpinButton (double min, double max, double step) : base (min, max, step)
		{
			Binding = new BindingControler<ySpinButton> (this, new Expression<Func<ySpinButton, object>>[] {
				(w => w.Value),
				(w => w.ValueAsInt),
				(w => w.ValueAsDecimal),
				(w => w.ValueAsShort),
			});
		}

		public ySpinButton (Adjustment adjustment, double climb_rate, uint digits) : base (adjustment, climb_rate, digits)
		{
			Binding = new BindingControler<ySpinButton> (this, new Expression<Func<ySpinButton, object>>[] {
				(w => w.Value),
				(w => w.ValueAsInt),
				(w => w.ValueAsDecimal),
				(w => w.ValueAsShort),
			});
		}

		protected override void OnValueChanged ()
		{
			Binding.FireChange (
				(w => w.Value),
				(w => w.ValueAsInt),
				(w => w.ValueAsDecimal),
				(w => w.ValueAsShort)
			);
			base.OnValueChanged ();
		}

		public decimal ValueAsDecimal {
			get { return Convert.ToDecimal (Value); }
			set { Value = Convert.ToDouble (value); }
		}

		new public int ValueAsInt {
			get { return Convert.ToInt32 (Value); }
			set { Value = Convert.ToDouble (value); }
		}

		public short ValueAsShort {
			get {
				if(Value > Int16.MaxValue)
					return Int16.MaxValue;
				if(Value < Int16.MinValue)
					return Int16.MinValue;

				return Convert.ToInt16(Value);
			}
			set { Value = Convert.ToDouble(value); }
		}

	}
}

