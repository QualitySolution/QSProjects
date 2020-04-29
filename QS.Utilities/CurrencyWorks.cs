using System;
namespace QS.Utilities
{
	public static class CurrencyWorks
	{
		public static string CurrencyShortName = "₽";

		public static string CurrencyShortFormat = "{0:N2} ₽";

		public static string GetShortCurrencyString(Decimal value)
		{
			return String.Format(CurrencyShortFormat, value);
		}

		public static string ToShortCurrencyString(this Decimal value)
		{
			return String.Format(CurrencyShortFormat, value);
		}
	}
}
