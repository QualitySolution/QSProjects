using System;
using NSubstitute;
using NUnit.Framework;
using QS.Project.Versioning;
using QS.Serial.Encoding;

namespace QS.Test.Serial
{
	[TestFixture(TestOf = typeof(SerialNumberEncoder))]
	public class SerialNumberEncoderTest
	{
		[Test(Description = "Тест набора серийных номеров версии 1")]
		[TestCase("PuWW-Hddh-2fjH-A2Sk-nC", true)]//АО "Первый хлебокомбинат"
		[TestCase("Pjr7-iiQj-vnW9-gNXb-oi", true)]//ООО «Беловопромжелдортранс»
		[TestCase("PaBj-9oB8-Bx6H-c3Vu-En", true)]//Охранное Агентство "Алекс-А"
		[TestCase("Nphy-bfeR-c9n2-sHn7-pZ", true)]//Сургутский РВПиС
		[TestCase("NaDP-kEYH-tof4-GE2M-Km", true)]//Сервисное локомотивное депо «Унеча»
		[TestCase("NVPC-U4DX-MS6S-w7J1-M5", true)]//ПСК Группы Компаний "Партнер"
		[TestCase("NVPC-U4DX-MS6S-w7J1-M6", false)]//Ошибочный.
		[TestCase("MVPC-U4DX-MS6S-w7J1-M5", false)]//Ошибочный.
		public void EncoderV1_WorkwearTest(string sn, bool isValid)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns("workwear");
			var encoder = new SerialNumberEncoder(appInfo);
			encoder.Number = sn;
			Assert.That(encoder.IsValid, Is.EqualTo(isValid));
		}
		
		[Test(Description = "Тест набора серийных номеров версии 1")]
		[TestCase("WPDi-ZMra-QnAe-jS", true, 1)]//Тестовый Однопользовательская
		[TestCase("WPDi-j6j5-65VB-HK", true, 2)]//Тестовый Профессиональная 2 пользователя
		[TestCase("Vw9k-kiEi-4LP7-23", true, 3)]//Тестовый Предприятие 1 пользователь
		[TestCase("WPDi-j6j5-88VB-HK", false, 0)]//Ошибочный.
		[TestCase("Vw9k-kiEi-5LP7-22", false, 0)]//Ошибочный.
		[TestCase("WPDi", false, 0)]//Ошибочный (набиваем покрытие:) )
		[TestCase("Aw9k-kiEi-5LP7-2222-1111-3333-5555-WW", false, 0)]//Ошибочный (набиваем покрытие:) )
		[TestCase("Vw9k-kiEi-5LP7-Андр-ей", false, 0)]//Ошибочный (набиваем покрытие:) )
		public void EncoderV2_WorkwearTest(string sn, bool isValid, byte edition)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductCode.Returns<byte>(2);
			var encoder = new SerialNumberEncoder(appInfo);
			encoder.Number = sn;
			Assert.That(encoder.IsValid, Is.EqualTo(isValid));
			if(isValid) {
				Assert.That(encoder.ProductId, Is.EqualTo(2));
				Assert.That(encoder.EditionId, Is.EqualTo(edition));
			}
		}
	}
}
