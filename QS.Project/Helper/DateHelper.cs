using System;
namespace QS.Helper
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

		public static void GetWeekPeriod(out DateTime startDate, out DateTime endDate, int weekIndex = 0)
		{
			DateTime date = DateTime.Now.Date.AddDays(weekIndex * 7);
			switch(date.DayOfWeek) {
				case DayOfWeek.Monday:
					startDate = date;
					endDate = date.AddDays(7);
					break;
				case DayOfWeek.Tuesday:
					startDate = date.AddDays(-1);
					endDate = date.AddDays(6);
					break;
				case DayOfWeek.Wednesday:
					startDate = date.AddDays(-2);
					endDate = date.AddDays(5);
					break;
				case DayOfWeek.Thursday:
					startDate = date.AddDays(-3);
					endDate = date.AddDays(4);
					break;
				case DayOfWeek.Friday:
					startDate = date.AddDays(-4);
					endDate = date.AddDays(3);
					break;
				case DayOfWeek.Saturday:
					startDate = date.AddDays(-5);
					endDate = date.AddDays(2);
					break;
				case DayOfWeek.Sunday:
					startDate = date.AddDays(-6);
					endDate = date.AddDays(1);
					break;
				default:
					startDate = DateTime.Now;
					endDate = DateTime.Now;
					break;
			}
		}

		public static void GetWeekPeriod(out DateTime? startDate, out DateTime? endDate , int weekIndex = 0)
		{
			DateTime startTempDate;
			DateTime endTempDate;
			GetWeekPeriod(out startTempDate, out endTempDate , weekIndex);
			startDate = startTempDate;
			endDate = endTempDate;
		}
	}
}
