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
	}
}
