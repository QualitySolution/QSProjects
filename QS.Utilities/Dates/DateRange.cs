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

		private DateTime FindExcludeEnd(DateRange range)
		{
			var next = ExcludedRanges.Where(r => r.Begin <= range.End && r.End > range.End).OrderBy(r => r.End).LastOrDefault();
			if (next == null)
				return range.End;
			else
				return FindExcludeEnd(next);
		}
	}
}
