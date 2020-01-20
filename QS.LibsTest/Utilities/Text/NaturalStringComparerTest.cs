using System;
using NUnit.Framework;
using QS.Utilities.Text;

namespace QS.Test.Utilities.Text
{
	[TestFixture(TestOf = typeof(NaturalStringComparer))]
	public class NaturalStringComparerTest
	{
		[Test(Description = "Различные кейсы")]
		[TestCase("А11", "А6", ExpectedResult = 1)]
		[TestCase("1-1", "1-6", ExpectedResult = -1)]
		[TestCase("1-6", "1-006", ExpectedResult = 0)]
		[TestCase("5.40ю", "5.040", ExpectedResult = 1)]//Реальный кейс, падали при сравнении в IndexOutOfRangeException
		public int CompareStringsTest(string a, string b)
		{
			return NaturalStringComparer.CompareStrings(a, b);
		}
	}
}
