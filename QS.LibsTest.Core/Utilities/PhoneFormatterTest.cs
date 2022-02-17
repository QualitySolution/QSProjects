using NUnit.Framework;
using QS.Utilities.Numeric;

namespace QS.Test.Utilities
{
	[TestFixture(TestOf = typeof(PhoneFormatter))]
	public class PhoneFormatterTest
	{
		#region RussiaOnlyHyphenated

		[Test(Description = "Проверяем что корректно конвертируем номер из других вариантов написания.")]
		[TestCase("+7(812)3098089")]
		[TestCase("+7(812)309-80-89")]
		[TestCase("+7(812) 309 80 89")]
		[TestCase("+7 812 309 - 80 - 89")]
		[TestCase("88123098089")] 
		[TestCase("78123098089")]
		[TestCase("+78123098089")]
		[TestCase("8-812-309-80-89")]
		[TestCase("8 812 309-8-089")]
		[TestCase("'+7-812-309-80-89'")]
		[TestCase(" \t 8-812-309-80-89")]
		[TestCase("fjklsj ds +7-8123098089")]
		public void RussiaOnlyHyphenated_ConvertFromAnotherFormatsTest(string insertPhone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			var result = phoneFormatter.FormatString(insertPhone);

			Assert.That(result, Is.EqualTo("+7-812-309-80-89"));
		}

		[Test(Description = "Проверяем что обрезаем лишнии цифры в номере.")]
		public void RussiaOnlyHyphenated_RemoveExtraDigitsTest()
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			var result = phoneFormatter.FormatString("+700011122334455");

			Assert.That(result, Is.EqualTo("+7-000-111-22-33"));
		}

		[Test(Description = "Проверяем что при вводе номера курсор останется на последней позиции.")]
		public void RussiaOnlyHyphenated_CursorOnEndTest()
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			int pos = 1;

			phoneFormatter.FormatString("8", ref pos);
			Assert.That(pos, Is.EqualTo(4));

			pos = 5;
			phoneFormatter.FormatString("+7812", ref pos);
			Assert.That(pos, Is.EqualTo(6));

			pos = 7;
			phoneFormatter.FormatString("+7-8123", ref pos);
			Assert.That(pos, Is.EqualTo(8));
		}

		[Test(Description = "Проверяем что курсор при вводе останется межу теми же цифрами что и раньше.")]
		public void RussiaOnlyHyphenated_CursorKeepLocation1Test()
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			//Курсор        
			//+78123098|089
			int pos = 9;
			phoneFormatter.FormatString("+78123098089", ref pos);
			//+7-812-309-8|0-89
			Assert.That(pos, Is.EqualTo(12));
		}

		[Test(Description = "Проверяем что курсор при вводе останется межу теми же цифрами что и раньше.")]
		public void RussiaOnlyHyphenated_CursorKeepLocation2Test()
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			//Курсор        
			//881230|98089
			int pos = 6;
			phoneFormatter.FormatString("88123098089", ref pos);
			//+7-812-30|9-80-89
			Assert.That(pos, Is.EqualTo(9));
		}

		[Test(Description = "Проверяем что курсор при вводе останется межу теми же цифрами что и раньше.")]
		public void RussiaOnlyHyphenated_CursorKeepLocation3Test()
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyHyphenated);
			//Курсор        
			//df r881230|98089
			int pos = 10;
			phoneFormatter.FormatString("df r88123098089", ref pos);
			//+7-812-30|9-80-89
			Assert.That(pos, Is.EqualTo(9));
		}

		#endregion
		#region BracketWithWhitespaceLastTen

		[Test(Description = "Проверяем что корректно конвертируем номер из других вариантов написания.")]
		[TestCase("+7(812)3098089")]
		[TestCase("+7(812)309-80-89")]
		[TestCase("+7(812) 309 80 89")]
		[TestCase("+7 812 309 - 80 - 89")]
		[TestCase("88123098089")]
		[TestCase("+78123098089")]
		[TestCase("8-812-309-80-89")]
		[TestCase("8 812 309-8-089")]
		[TestCase("'+7-812-309-80-89'")]
		[TestCase(" \t 8-812-309-80-89")]
		[TestCase("fjklsj ds +7-8123098089")]
		[TestCase("78123098089")]
		public void BracketWithWhitespaceLastTen_ConvertFromAnotherFormatsTest(string insertPhone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
			var result = phoneFormatter.FormatString(insertPhone);

			Assert.That(result, Is.EqualTo("(812) 309 - 80 - 89"));
		}

		[Test(Description = "Проверяем что корректно конвертируем номер из других вариантов написания.")]
		[TestCase("78536600012", "(853) 660 - 00 - 12")]
		[TestCase("88536600012", "(853) 660 - 00 - 12")]
		[TestCase( "8536600012", "(853) 660 - 00 - 12")]
		[TestCase( "7536600012", "(753) 660 - 00 - 12")]
		public void BracketWithWhitespaceLastTen_ConvertFromAnotherFormatsTest1(string insertPhone, string result)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.BracketWithWhitespaceLastTen);
			var _result = phoneFormatter.FormatString(insertPhone);

			Assert.That(_result, Is.EqualTo(result));
		}
		#endregion

		#region RussiaOnlyShort

		[Test(Description = "Проверяем что корректно конвертируем номер из других вариантов написания.")]
		[TestCase("+7(812)3098089")]
		[TestCase("+7(812)309-80-89")]
		[TestCase("+7(812) 309 80 89")]
		[TestCase("+7 812 309 - 80 - 89")]
		[TestCase("88123098089")]
		[TestCase("+78123098089")]
		[TestCase("8-812-309-80-89")]
		[TestCase("8 812 309-8-089")]
		[TestCase("'+7-812-309-80-89'")]
		[TestCase(" \t 8-812-309-80-89")]
		[TestCase("fjklsj ds +7-8123098089")]
		[TestCase("78123098089")]
		public void RussiaOnlyShort_ConvertFromAnotherFormatsTest(string insertPhone)
		{
			var phoneFormatter = new PhoneFormatter(PhoneFormat.RussiaOnlyShort);
			var result = phoneFormatter.FormatString(insertPhone);

			Assert.That(result, Is.EqualTo("+78123098089"));
		}
		#endregion
	}
}
