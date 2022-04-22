using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Utilities.Dates
{
	public class DateRange
	{
		public DateTime Begin { get; private set; }
		public DateTime End { get; private set; }

		public List<DateRange> ExcludedRanges { get; set; }

		public DateRange(DateTime begin, DateTime end)
		{
			//Мы работаем всегда чисто датами, без времени. 
			//Поэтому не даем возможность внести случайно дату со временем.
			Begin = begin.Date;
			End = end.Date;

			ExcludedRanges = new List<DateRange>();
		}

		public IEnumerable<DateRange> GetIntervals{
			get {
				var start = Begin;
				while(start <= End) {
					var endCurrentExclude = FindEndOfExclusion(start);
					if(endCurrentExclude != null)
						start = endCurrentExclude.Value.AddDays(1);
					if (start > End)
						yield break;

					var nextExclude = ExcludedRanges.Where(r => r.Begin > start).OrderBy(r => r.Begin).FirstOrDefault();
					DateTime end;
					if(nextExclude == null || nextExclude.Begin > End)
						end = End;
					else
						end = nextExclude.Begin.AddDays(-1);

					yield return new DateRange(start, end);
					start = end.AddDays(1);
				};
			}
		}

		public int Days => (End - Begin).Days + 1;

		public int Months => ((End.Year - Begin.Year) * 12) + End.Month - Begin.Month;

		public int GetTotalExcludedDays{
			get {
				var intervals = GetIntervals.ToArray();
				if (intervals.Length <= 1)
					return 0;
				int days = 0;
				for(int i = 1; i < intervals.Length; i++)
				{
					days += (intervals[i].Begin - intervals[i - 1].End).Days - 1;
				}
				return days;
			}
		}

		private DateTime FindExcludeEnd(DateRange range)
		{
			//К концу диапазона добавлен день, для того чтобы склеивать случаи когда 5 числа заканчивается предыдущий, а 6-го начинается следующий.
			var next = ExcludedRanges.Where(r => r.Begin <= range.End.AddDays(1) && r.End > range.End).OrderBy(r => r.End).LastOrDefault();
			if (next == null)
				return range.End;
			else
				return FindExcludeEnd(next);
		}

		public DateTime? FindEndOfExclusion(DateTime pointInExcludeRange)
		{
			var inExclude = ExcludedRanges.Where(r => r.Begin <= pointInExcludeRange && r.End >= pointInExcludeRange).OrderBy(r => r.End).LastOrDefault();
			if(inExclude != null)
				return FindExcludeEnd(inExclude);
			else
				return null;
		}

		public DateTime FillIntervals(int days)
		{
			foreach(var interval in GetIntervals)
			{
				if (interval.Days >= days)
					return interval.Begin.AddDays(days);

				days = days - interval.Days;
			}
			throw new ArgumentOutOfRangeException(nameof(days), "Количество дней для заполнения превысило общее количество дней в интервалах.");
		}
	}
}
