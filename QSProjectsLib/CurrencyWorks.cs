using System;

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
}

