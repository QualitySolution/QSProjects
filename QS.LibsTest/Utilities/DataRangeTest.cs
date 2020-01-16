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

		[Test(Description = "Проверяем расчет количества дней в исключенных пересекаемых диапазонов")]
		public void GetTotalExcludedDays_ExcludeIntersectedDaysTest()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 2, 5), new DateTime(2019, 2, 9));
			var exclude2 = new DateRange(new DateTime(2019, 2, 8), new DateTime(2019, 2, 14));

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);

			Assert.That(mainRange.GetTotalExcludedDays, Is.EqualTo(10));
		}

		[Test(Description = "Проверяем расчет количества дней в исключенных пересекаемых диапазонов")]
		public void GetTotalExcludedDays_MultiExcludeIntersectedDaysTest()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 2, 5), new DateTime(2019, 2, 9));
			var exclude2 = new DateRange(new DateTime(2019, 2, 8), new DateTime(2019, 2, 14));
			var exclude3 = new DateRange(new DateTime(2019, 4, 5), new DateTime(2019, 4, 9));

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);
			mainRange.ExcludedRanges.Add(exclude3);

			Assert.That(mainRange.GetTotalExcludedDays, Is.EqualTo(15));
		}

		[Test(Description = "Проверяем расчет количества дней в ситуации когда дипазоны косаются боками, между ними нет дней.")]
		public void GetTotalExcludedDays_ExcludeSideTouchDaysTest()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 2, 5), new DateTime(2019, 2, 9));
			var exclude2 = new DateRange(new DateTime(2019, 2, 10), new DateTime(2019, 2, 14));

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);

			Assert.That(mainRange.GetTotalExcludedDays, Is.EqualTo(10));
		}

		[Test(Description = "Заполнение интервалов днями.")]
		public void FillIntervals_Test()
		{
			var mainRange = new DateRange(new DateTime(2019, 1, 1), new DateTime(2019, 5, 5));
			var exclude = new DateRange(new DateTime(2019, 1, 5), new DateTime(2019, 1, 6));//Два дня
			var exclude2 = new DateRange(new DateTime(2019, 1, 10), new DateTime(2019, 1, 12));//Три дня

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);

			// + 5 дней
			Assert.That(mainRange.FillIntervals(20), Is.EqualTo(new DateTime(2019, 1, 26)));
		}

		[Test(Description = "Игнорирование времени при заполнение интервалов.")]
		[Category("real case")]
		public void FillIntervals_IgnogeTimeTest()
		{
			var mainRange = new DateRange(new DateTime(2018, 11, 22, 15, 09, 23), DateTime.MaxValue);
			var exclude = new DateRange(new DateTime(2019, 7, 19), new DateTime(2019, 7, 23));//5 дней
			var exclude2 = new DateRange(new DateTime(2018, 7, 3), new DateTime(2018, 7, 5));//Три дня

			mainRange.ExcludedRanges.Add(exclude);
			mainRange.ExcludedRanges.Add(exclude2);

			// + 5 дней
			Assert.That(mainRange.FillIntervals(365), Is.EqualTo(new DateTime(2019, 11, 27)));
		}

		private static readonly object[] MonthsCalculationCases = new object[] {
			new object[] {new DateRange(new DateTime(2019, 12, 20), new DateTime(2020, 12, 20)), 12},
			new object[] {new DateRange(new DateTime(2018, 12, 20), new DateTime(2020, 12, 20)), 24},
			new object[] {new DateRange(new DateTime(2020, 12, 20), new DateTime(2019, 12, 20)), -12},
			new object[] {new DateRange(new DateTime(2019, 11, 20), new DateTime(2019, 12, 20)), 1},
			new object[] {new DateRange(new DateTime(2018, 12, 20), new DateTime(2020, 10, 20)), 22},
			new object[] {new DateRange(new DateTime(2018, 10, 10), new DateTime(2018, 10, 20)), 0},
		};

		[Test(Description = "Проверка подсчета месяцев")]
		[TestCaseSource(nameof(MonthsCalculationCases))]
		public void MonthsCalculationTest(DateRange range, int result)
		{
			Assert.That(range.Months, Is.EqualTo(result));
		}
	}
}
