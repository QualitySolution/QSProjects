using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Utilities.Dates
{
	public class DateRange
	{
		public DateTime Begin { get; set; }
		public DateTime End { get; set; }

		public List<DateRange> ExcludedRanges { get; set; }

		public DateRange(DateTime begin, DateTime end)
		{
			Begin = begin;
			End = end;

			ExcludedRanges = new List<DateRange>();
		}

		public IEnumerable<DateRange> GetIntervals{
			get {
				var start = Begin;
				while(start <= End) {
					var currentExclude = ExcludedRanges.Where(r => r.Begin <= start && r.End >= start).OrderBy(r => r.End).LastOrDefault();
					if(currentExclude != null)
						start = FindExcludeEnd(currentExclude).AddDays(1);
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
			//К концу дипазона добавлен день, для того чтобы склеивать случаи когда 5 числа заканчивается предыдущий, а 6-го начинается следующий.
			var next = ExcludedRanges.Where(r => r.Begin <= range.End.AddDays(1) && r.End > range.End).OrderBy(r => r.End).LastOrDefault();
			if (next == null)
				return range.End;
			else
				return FindExcludeEnd(next);
		}
	}
}
