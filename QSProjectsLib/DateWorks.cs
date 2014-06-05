using System;

namespace QSProjectsLib
{
	public class DateWorks
	{
		/*public DateWorks()
		{
		}*/

		public static string GetDateRangeText(DateTime dateS, DateTime dateE)
		{
			if(dateS == default(DateTime) || dateE == default(DateTime))
				return "";
			if (dateS.Year != dateE.Year)
				return String.Format("{0:D}–{1:D}", dateS, dateE);
			else if (dateS.Month != dateE.Month)
				return String.Format("{0:dd MMMMM}–{1:D}", dateS, dateE);
			else
				return String.Format("{0:dd}–{1:D}", dateS, dateE);

		}
	}
}

