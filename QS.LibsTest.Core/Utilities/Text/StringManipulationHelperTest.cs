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

		[Test(Description = "Проверяем обработку уменьшения строки именно по середине")]
		public void EllipsizeMiddle_SinkSize() {
			var result = StringManipulationHelper.EllipsizeMiddle(
				"Я помню чудное мгновенье: Передо мной явилась ты, Как мимолетное виденье, Как гений чистой красоты. В томленьях грусти безнадежной, В тревогах шумной суеты, Звучал мне долго голос нежный И снились милые черты.",
				100);
			Assert.That(result, Has.Length.EqualTo(100));
			Assert.That(result, Does.StartWith("Я помню чудное мгновенье"));
			Assert.That(result, Does.EndWith("И снились милые черты."));
			var parts = result.Split("…");
			Assert.That(parts[0], Has.Length.LessThanOrEqualTo(50));
			Assert.That(parts[1], Has.Length.LessThanOrEqualTo(50));
		}
		
		[Test(Description = "Проверяем что ничего не делаем со строкой если она меньше максимальной длинны")]
		public void EllipsizeMiddle_DontSinkSize() {
			var result = StringManipulationHelper.EllipsizeMiddle(
				"Век живи — век учись тому, как следует жить",
				100);
			Assert.That(result, Has.Length.EqualTo(43));
		}
		
		[Test(Description = "Проверяем что ничего не делаем со строкой если той же длинны что требуется.")]
		public void EllipsizeMiddle_DontSinkSize_EqualLengthCase() {
			var result = StringManipulationHelper.EllipsizeMiddle(
				"Делу - время, потехе - час.",
				27);
			Assert.That(result, Has.Length.EqualTo(27));
			Assert.That(result, Does.Not.Contain("…"));
		}
	}
}
