using NUnit.Framework;
using QS.Utilities.Dates;
using System;
using System.Linq;

namespace QS.Test.Utilities
{
	[TestFixture(TestOf = typeof(DateRange))]
	public class DataRangeTest
	{
		[Test(Description = "Проверяем исключение диапазона")]
		public void GetIntervals_ExcludeTest()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 2, 5), new DateTime(2019, 2, 10));
			mainRange.ExcludedRanges.Add(exclude);

			var intervals = mainRange.GetIntervals.ToArray();
			Assert.That(intervals[0].Begin, Is.EqualTo(new DateTime(2019, 1, 1)));
			Assert.That(intervals[0].End, Is.EqualTo(new DateTime(2019, 2, 4)));

			Assert.That(intervals[1].Begin, Is.EqualTo(new DateTime(2019, 2, 11)));
			Assert.That(intervals[1].End, Is.EqualTo(new DateTime(2019, 5, 5)));
		}

		[Test(Description = "Проверяем исключение пересекаемых диапазонов")]
		public void GetIntervals_ExcludeIntersectedTest()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 2, 5), new DateTime(2019, 2, 28));
			var exclude2 = new DateRange(new DateTime(2019, 2, 15), new DateTime(2019, 3, 10));

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);

			var intervals = mainRange.GetIntervals.ToArray();
			Assert.That(intervals[0].Begin, Is.EqualTo(new DateTime(2019, 1, 1)));
			Assert.That(intervals[0].End, Is.EqualTo(new DateTime(2019, 2, 4)));

			Assert.That(intervals[1].Begin, Is.EqualTo(new DateTime(2019, 3, 11)));
			Assert.That(intervals[1].End, Is.EqualTo(new DateTime(2019, 5, 5)));
		}
	}
}
