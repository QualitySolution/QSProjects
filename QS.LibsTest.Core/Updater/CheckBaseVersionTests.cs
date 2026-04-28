using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.BaseParameters;
using QS.Project.Versioning;

namespace QS.Test.Updater
{
	/// <summary>
	/// Тесты для CheckBaseVersion
	/// </summary>
	[TestFixture(TestOf = typeof(CheckBaseVersion))]
	public class CheckBaseVersionTests
	{
		/// <summary>
		/// Тест: название продукта в базе не указано — должно быть сообщение об ошибке.
		/// </summary>
		[Test(Description = "Название продукта в базе не указано — возвращает IncorrectProduct")]
		public void Check_WhenProductNameIsEmpty_ShouldReturnIncorrectProduct()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), Array.Empty<string>());
			var parameters = CreateParameters("", "2.8", null);
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.IncorrectProduct, checker.Result);
		}

		/// <summary>
		/// Тест: название продукта из другого продукта — должно быть сообщение об ошибке.
		/// </summary>
		[Test(Description = "Название продукта в базе от другого продукта — возвращает IncorrectProduct")]
		public void Check_WhenProductNameMismatch_ShouldReturnIncorrectProduct()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), Array.Empty<string>());
			var parameters = CreateParameters("OtherProduct", "2.8", null);
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.IncorrectProduct, checker.Result);
		}

		/// <summary>
		/// Тест: версия базы данных не указана — должно быть сообщение об ошибке.
		/// </summary>
		[Test(Description = "Версия базы данных не указана — возвращает IncorrectVersion")]
		public void Check_WhenDatabaseVersionIsEmpty_ShouldReturnIncorrectVersion()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), Array.Empty<string>());
			var parameters = CreateParameters("TestProduct", "", null);
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.IncorrectVersion, checker.Result);
		}

		/// <summary>
		/// Тест: обычный кейс когда программа новее базы (BaseVersionLess), редакции у программы нет.
		/// </summary>
		[Test(Description = "Программа 2.8 новее базы 2.7 — возвращает BaseVersionLess (без редакций)")]
		public void Check_WhenAppVersionGreaterThanBase_ShouldReturnBaseVersionLess()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), Array.Empty<string>());
			var parameters = CreateParameters("TestProduct", "2.7", null);
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.BaseVersionLess, checker.Result);
		}

		/// <summary>
		/// Тест: программа поддерживает только редакцию "sps", а в базе редакция не указана.
		/// Должно быть сообщение об ошибке редакции.
		/// </summary>
		[Test(Description = "Программа поддерживает только 'sps', в базе редакция не указана — возвращает UnsupportedEdition")]
		public void Check_WhenAppSupportsSpsOnly_AndBaseEditionIsEmpty_ShouldReturnUnsupportedEdition()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), new[] { "sps" });
			var parameters = CreateParameters("TestProduct", "2.8", "");
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.UnsupportedEdition, checker.Result);
		}

		/// <summary>
		/// Тест: программа поддерживает редакции "sps" и null (без редакции), в базе редакция не указана.
		/// Сообщение об ошибке редакции НЕ должно появляться.
		/// </summary>
		[Test(Description = "Программа поддерживает 'sps' и пустую редакцию, в базе редакция не указана — сообщения нет")]
		public void Check_WhenAppSupportsSpsAndNull_AndBaseEditionIsEmpty_ShouldNotReturnEditionError()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), new[] { "sps", null });
			var parameters = CreateParameters("TestProduct", "2.8", "");
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsFalse(hasMessage);
			Assert.AreEqual(CheckBaseResult.Ok, checker.Result);
		}

		/// <summary>
		/// Тест: аналогично предыдущему — в базе редакция равна 2 пробелам, программа поддерживает "sps" и null.
		/// Сообщение об ошибке редакции НЕ должно появляться.
		/// </summary>
		[Test(Description = "Программа поддерживает 'sps' и пустую редакцию, в базе редакция равна двум пробелам — сообщения нет")]
		public void Check_WhenAppSupportsSpsAndNull_AndBaseEditionIsTwoSpaces_ShouldNotReturnEditionError()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), new[] { "sps", null });
			var parameters = CreateParameters("TestProduct", "2.8", "  ");
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsFalse(hasMessage);
			Assert.AreEqual(CheckBaseResult.Ok, checker.Result);
		}

		/// <summary>
		/// Тест: в базе редакция — null, а в поддерживаемых редакциях программы "sps" и строка из двух пробелов.
		/// Сообщение об ошибке редакции НЕ должно появляться.
		/// </summary>
		[Test(Description = "В базе редакция null, программа поддерживает 'sps' и '  ' — сообщения нет")]
		public void Check_WhenAppSupportsSpsAndTwoSpaces_AndBaseEditionIsNull_ShouldNotReturnEditionError()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), new[] { "sps", "  " });
			var parameters = CreateParameters("TestProduct", "2.8", null);
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsFalse(hasMessage);
			Assert.AreEqual(CheckBaseResult.Ok, checker.Result);
		}

		/// <summary>
		/// Тест: в базе редакция "aros", программа поддерживает "sps" и null.
		/// Должно быть сообщение об ошибке редакции.
		/// </summary>
		[Test(Description = "В базе редакция 'aros', программа поддерживает 'sps' и пустую — возвращает UnsupportedEdition")]
		public void Check_WhenBaseEditionIsAros_AndAppSupportsSpsAndNull_ShouldReturnUnsupportedEdition()
		{
			var appInfo = CreateApplicationInfo("TestProduct", new Version(2, 8, 5), new[] { "sps", null });
			var parameters = CreateParameters("TestProduct", "2.8", "aros");
			var checker = new CheckBaseVersion(appInfo, parameters);

			var hasMessage = checker.Check();

			Assert.IsTrue(hasMessage);
			Assert.AreEqual(CheckBaseResult.UnsupportedEdition, checker.Result);
		}

		#region Вспомогательные методы

		private IApplicationInfo CreateApplicationInfo(string productName, Version version, string[] compatibleModifications)
		{
			var appInfo = Substitute.For<IApplicationInfo>();
			appInfo.ProductName.Returns(productName);
			appInfo.Version.Returns(version);
			appInfo.CompatibleModifications.Returns(compatibleModifications);
			return appInfo;
		}

		private ParametersService CreateParameters(string productName, string version, string edition)
		{
			var dict = new Dictionary<string, string>
			{
				{ "product_name", productName ?? "" },
				{ "version", version ?? "" }
			};
			if(edition != null)
				dict["edition"] = edition;
			return new ParametersService(dict);
		}

		#endregion
	}
}

