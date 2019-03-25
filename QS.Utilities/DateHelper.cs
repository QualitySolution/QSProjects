using System;
namespace QS.Utilities
{
	public static class DateHelper
	{
		public static string GetDateRangeText(DateTime dateS, DateTime dateE)
		{
			if(dateS == default(DateTime) || dateE == default(DateTime))
				return "";
			if(dateS.Year != dateE.Year)
				return String.Format("{0:D}–{1:D}", dateS, dateE);
			else if(dateS.Month != dateE.Month)
				return String.Format("{0:dd MMMMM}–{1:D}", dateS, dateE);
			else
				return String.Format("{0:dd}–{1:D}", dateS, dateE);

		}

		public static string GetMonthName(int monthNumber)
		{
			var tempDate = new DateTime(2015, monthNumber, 1);
			return tempDate.ToString("MMMM");
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
