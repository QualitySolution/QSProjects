using System;
using System.ComponentModel;
using Gamma.Binding.Core;
using System.Linq.Expressions;

namespace QSOrmProject
{
	[ToolboxItem(true)]
	[Category ("Gamma Widgets")]
	public partial class DisableSpinButton : Gtk.Bin
	{
		public BindingControler<DisableSpinButton> Binding { get; private set;}

		public bool Active{
			get{
				return check.Active;
			}
			set{
				if(check.Active != value)
					check.Active = value;
				spinbutton.Sensitive = value;
			}
		}

		public double? Value{
			get{
				return Active ? spinbutton.Value : (double?)null;
			}
			set{
				Active = value != null;
				if(value != null)
					spinbutton.Value = value.Value;
			}
		}

		public int? ValueAsInt{
			get{
				return Active ? spinbutton.ValueAsInt : (int?)null;
			}
			set{
				Active = value != null;
				if(value != null)
					spinbutton.Value = value.Value;
			}
		}

		public decimal? ValueAsDecimal{
			get{
				return Active ? Convert.ToDecimal(spinbutton.Value) : (decimal?)null;
			}
			set{
				Active = value != null;
				if(value != null)
					spinbutton.Value = Convert.ToDouble (value.Value);
			}
		}

		public double Upper{
			get{
				return spinbutton.Adjustment.Upper;
			}
			set{
				spinbutton.Adjustment.Upper = value;
			}
		}

		public double Lower{
			get{
				return spinbutton.Adjustment.Lower;
			}
			set{
				spinbutton.Adjustment.Lower = value;
			}
		}

		public uint Digits{
			get{
				return spinbutton.Digits;
			}
			set{
				spinbutton.Digits = value;
			}
		}

		public DisableSpinButton()
		{
			this.Build();

			Binding = new BindingControler<DisableSpinButton> (this, new Expression<Func<DisableSpinButton, object>>[] {
				(w => w.Active),
				(w => w.Value),
				(w => w.ValueAsInt),
				(w => w.ValueAsDecimal)
			});
		}

		protected void OnCheckToggled(object sender, EventArgs e)
		{
			spinbutton.Sensitive = Active;

			Binding.FireChange (new Expression<Func<DisableSpinButton, object>>[] {
				(w => w.Active),
				(w => w.Value),
				(w => w.ValueAsDecimal),
				(w => w.ValueAsInt),
			});
		}

		protected void OnSpinbuttonChanged(object sender, EventArgs e)
		{
			Binding.FireChange (new Expression<Func<DisableSpinButton, object>>[] {
				(w => w.Value),
				(w => w.ValueAsDecimal),
				(w => w.ValueAsInt),
			});
		}
	}
}

