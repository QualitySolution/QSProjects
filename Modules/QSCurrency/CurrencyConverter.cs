using System;
using System.Collections.Generic;

namespace QSCurrency.CBR
{
	public static class CurrencyConverter
	{
		static List<CurrencyRate> ratesCBR;

		public static List<CurrencyRate> RatesCBR
		{
			get
			{
				if(ratesCBR == null)
					ratesCBR = CurrencyRates.GetExchangeRates();
				return ratesCBR;
			}
		}

		public static decimal? Convert(decimal money, string inCurrency, string toCurrency)
		{
			decimal inRub;
			if (inCurrency == "RUB")
				inRub = money;
			else
			{
				var rate = RatesCBR.Find(x => x.CurrencyStringCode == inCurrency);
				if (rate == null)
					return null;
				inRub = money * rate.ExchangeRate;
			}

			if (toCurrency == "RUB")
				return inRub;
			else
			{
				var rate = RatesCBR.Find(x => x.CurrencyStringCode == toCurrency);
				if (rate == null)
					return null;
				
				return inRub / rate.ExchangeRate;
			}
		}
	}
}

