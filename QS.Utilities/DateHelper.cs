using System;
namespace QS.Utilities
{
	public static class DateHelper
	{
		/// <summary>
		/// Возвращает текстовое представление диапазона дат.
		/// В вариантах:
		/// "1 января 2015" – если даты начала и конца периода одинаковая. То есть период в один день.
		/// "1-25 января 2015" – если даты начала и конца периода в одном месяце.
		/// "1 января – 25 февраля 2015" – если даты начала и конца периода в разных месяцах.
		/// "1 января 2015 – 25 февраля 2020" – если даты начала и конца периода в разных годах.
		/// </summary>
		public static string GetDateRangeText(DateTime? dateStart, DateTime? dateEnd)
		{
			if(dateStart == default(DateTime) || dateEnd == default(DateTime) || dateStart == null || dateEnd == null)
				return String.Empty;
			if(dateStart.Value.Year != dateEnd.Value.Year)
				return $"{dateStart:d MMMM yyyy} – {dateEnd:d MMMM yyyy}";
			if(dateStart.Value.Month != dateEnd.Value.Month)
				return $"{dateStart:d MMMM} – {dateEnd:d MMMM yyyy}";
			if(dateStart.Value.Day != dateEnd.Value.Day)
				return $"{dateStart.Value.Day}–{dateEnd:d MMMM yyyy}";
			
			return $"{dateStart:d MMMM yyyy}";
		}

		/// <summary>
		/// Возвращает название месяца.
		/// </summary>
		public static string GetMonthName(int monthNumber)
		{
			var tempDate = new DateTime(2015, monthNumber, 1);
			return tempDate.ToString("MMMM");
		}

		/// <summary>
		/// Возвращает название месяца в родительном падеже.
		/// </summary>
		public static string GetMonthGenitiveName(int monthNumber) {
			string[] monthsGenitive = { "января", "февраля", "марта", "апреля", "мая", "июня", 
				"июля", "августа", "сентября", "октября", "ноября", "декабря" };
			return monthsGenitive[monthNumber - 1];
		}

		public static void GetWeekPeriod(out DateTime startDate, out DateTime endDate, DateTime date)
		{
			switch(date.DayOfWeek) {
				case DayOfWeek.Monday:
					startDate = date.Date;
					endDate = date.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Tuesday:
					startDate = date.AddDays(-1).Date;
					endDate = date.AddDays(5).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Wednesday:
					startDate = date.AddDays(-2).Date;
					endDate = date.AddDays(4).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Thursday:
					startDate = date.AddDays(-3).Date;
					endDate = date.AddDays(3).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Friday:
					startDate = date.AddDays(-4).Date;
					endDate = date.AddDays(2).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Saturday:
					startDate = date.AddDays(-5).Date;
					endDate = date.AddDays(1).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				case DayOfWeek.Sunday:
					startDate = date.AddDays(-6).Date;
					endDate = date.AddDays(0).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
					break;
				default:
					startDate = DateTime.MinValue;
					endDate = DateTime.MinValue;
					break;
			}
		}

		public static void GetWeekPeriod(out DateTime startDate, out DateTime endDate, int weekIndex = 0)
		{
			GetWeekPeriod(out startDate, out endDate, DateTime.Now.Date.AddDays(weekIndex * 7));
		}

		public static void GetWeekPeriod(out DateTime? startDate, out DateTime? endDate, int weekIndex = 0)
		{
			GetWeekPeriod(out DateTime startTempDate, out DateTime endTempDate, weekIndex);
			startDate = startTempDate;
			endDate = endTempDate;
		}
	}
}
