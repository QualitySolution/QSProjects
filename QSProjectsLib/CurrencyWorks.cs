using System;
using System.ComponentModel;
using Gtk;

namespace QSProjectsLib
{
	public static class CurrencyWorks
	{
		public static string CurrencyShortName = "₽";

		public static string CurrencyShortFomat = "{0:N2} ₽";

		public static string GetShortCurrencyString (Decimal value)
		{
			return String.Format (CurrencyShortFomat, value);
		}
	}

	[ToolboxItem (true)]
	public class CurrencyLabel : Label
	{
		public CurrencyLabel ()
		{
			Text = LabelProp = CurrencyWorks.CurrencyShortName;
		}

		new public string LabelProp {
			get { return base.LabelProp; }
			set { }
		}
	}
}

