using GLib;
using Gtk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamma.GtkWidgets 
{
	[System.ComponentModel.ToolboxItem(true)]
	[System.ComponentModel.Category("Gamma Gtk")]
	public class ySpinButtonCurrencyFormat : ySpinButton 
	{
		public ySpinButtonCurrencyFormat(double min, double max, double step) : base(min, max, step) { }

		public ySpinButtonCurrencyFormat(Adjustment adjustment, double climb_rate, uint digits) : base(adjustment, climb_rate, digits) { }

		public bool CurrencyFormat = true;

		protected override int OnInput(out double new_value) 
		{
			if(CurrencyFormat) 
			{
				if(Text.Length == 0) 
				{
					Text = "0";
				}

				if(!double.TryParse(Text.Replace(" ", ""), NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out new_value)) 
				{
					new_value = Value;
				}
				return 1;
			}
			return base.OnInput(out new_value);
		}

		protected override int OnOutput() 
		{
			if(CurrencyFormat) 
			{
				Numeric = false;

				NumberFormatInfo valueFormatInfo = new NumberFormatInfo();
				valueFormatInfo.NumberDecimalSeparator = ".";
				valueFormatInfo.NumberGroupSeparator = " ";

				Text = string.Format(valueFormatInfo, "{0:#,0.00}", Value);
				Numeric = true;

				return 1;
			}
			return base.OnOutput();
		}
	}
}
