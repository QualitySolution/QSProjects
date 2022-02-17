using System;
using NUnit.Framework;
using QS.Utilities.Text;

namespace QS.Test.Utilities.Text
{
	[TestFixture(TestOf = typeof(StringManipulationHelper))]
	public class StringManipulationHelperTest
	{
		[Test(Description = "Различные кейсы")]
		[TestCase("https://shopvodovoz.uberserver.ru/bitrix/admin/", "/", "", ExpectedResult = "https://shopvodovoz.uberserver.ru/bitrix/admin")]
		[TestCase("https://shopvodovoz.uberserver.ru/bitrix/admin/1c_exchange.php", "1c_exchange.php", "", ExpectedResult = "https://shopvodovoz.uberserver.ru/bitrix/admin/")]
		[TestCase("https://shopvodovoz.uberserver.ru/bitrix/admin/", "1c_exchange.php", "", ExpectedResult = "https://shopvodovoz.uberserver.ru/bitrix/admin/")]
		public string ReplaceLastOccurrenceTest(string Source, string Find, string Replace)
		{
			return StringManipulationHelper.ReplaceLastOccurrence(Source, Find, Replace);
		}
	}
}
