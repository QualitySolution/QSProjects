﻿using System;
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
			if(isValid)
				Assert.That(encoder.CodeVersion, Is.EqualTo(1));
		}
		
		[Test(Description = "Тест набора серийных номеров версии 2")]
		[TestCase("WPDi-ZMra-QnAe-jS", true, 1, (ushort)11, null)]//Тестовый Однопользовательская
		[TestCase("XgAc-j7gz-2pJ3-J7", true, 2, (ushort)34, null)]//Тестовый Профессиональная 2 пользователя
		[TestCase("XgAc-tpMz-qdne-fM", true, 3, (ushort)34, null)]//Тестовый Предприятие 1 пользователь
		[TestCase("BJbw-eP4K-gvRn-aXtD-5", true, 4, (ushort)34, "2024-10-03")]//Тестовый Спецаутсорсинг 1 пользователь с датой окончания
		[TestCase("WPDi-j6j5-88VB-HK", false, 0, (ushort)0, null)]//Ошибочный.
		[TestCase("Vw9k-kiEi-5LP7-22", false, 0, (ushort)0, null)]//Ошибочный.
		[TestCase("WPDi", false, 0, (ushort)0, null)]//Ошибочный (набиваем покрытие:) )
		[TestCase("Aw9k-kiEi-5LP7-2222-1111-3333-5555-WW", false, 0, (ushort)0, null)]//Ошибочный (набиваем покрытие:) )
		[TestCase("Vw9k-kiEi-5LP7-Андр-ей", false, 0, (ushort)0, null)]//Ошибочный (набиваем покрытие:) )
		public void EncoderV2_WorkwearTest(string sn, bool isValid, byte edition, ushort clientId, DateTime? expiryDate)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductCode.Returns<byte>(2);
			var encoder = new SerialNumberEncoder(appInfo);
			encoder.Number = sn;
			Assert.That(encoder.IsValid, Is.EqualTo(isValid));
			if(isValid)
				Assert.That(encoder.CodeVersion, Is.EqualTo(2));
			if(isValid) {
				Assert.That(encoder.ProductId, Is.EqualTo(2));
				Assert.That(encoder.EditionId, Is.EqualTo(edition));
				Assert.That(encoder.ClientId, Is.EqualTo(clientId));
				Assert.That(encoder.ExpiryDate, Is.EqualTo(expiryDate));
			}
		}
		
		[Test(Description = "Тест набора серийных номеров версии 3")]
		[TestCase("Fhd9-nuXE-UWLQ-7eW3-w", true, 1, (ushort)11,(ushort)6, null)]//Тестовый Однопользовательская
		[TestCase("G8oE-6zju-qHUb-8Cqf-c", true, 2, (ushort)34,(ushort)15, null)]//Тестовый Профессиональная 2 пользователя
		[TestCase("G8oE-6zju-qHUb-8Cqk-B", true, 3, (ushort)34, (ushort)15, null)]//Тестовый Предприятие 1 пользователь
		[TestCase("65qs-56f8-nnsu-Goat-8Pmb", true, 4, (ushort)34, (ushort)10, "2024-10-04")]//Тестовый Спецаутсорсинг 1 пользователь с датой окончания
		public void EncoderV3_WorkwearTest(string sn, bool isValid, byte edition, ushort clientId, ushort employees, DateTime? expiryDate)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductCode.Returns<byte>(2);
			var encoder = new SerialNumberEncoder(appInfo);
			encoder.Number = sn;
			Assert.That(encoder.IsValid, Is.EqualTo(isValid));
			if(isValid)
				Assert.That(encoder.CodeVersion, Is.EqualTo(3));
			if(isValid) {
				Assert.That(encoder.ProductId, Is.EqualTo(2));
				Assert.That(encoder.EditionId, Is.EqualTo(edition));
				Assert.That(encoder.ClientId, Is.EqualTo(clientId));
				Assert.That(encoder.Employees, Is.EqualTo(employees));
				Assert.That(encoder.ExpiryDate, Is.EqualTo(expiryDate));
			}
		}
	}
}
