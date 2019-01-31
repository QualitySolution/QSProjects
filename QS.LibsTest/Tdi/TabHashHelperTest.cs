using NSubstitute;
using NUnit.Framework;
using QS.Dialog.Gtk;
using QS.Tdi;
using System;
namespace QS.Test.Tdi
{
	[TestFixture(TestOf = typeof(TabHashHelper))]
	public class TabHashHelperTest
	{
		[Test(Description = "Проверяем что когда ищем методы получения хеша не обращаем внимания на generic методы с тем же аргументами.")]
		public void GetTabHash_NotGetGenericMethodsCase()
		{
			var hash = TabHashHelper.GetTabHash(typeof(TabTestClass), new Type[] { }, new object[] { });
			Assert.That(hash, Is.EqualTo("good_hash"));
		}
	}

	public class TabTestClass : TdiTabBase
	{
		public static string GenerateHashName<TArg>()
		{
			return "bad_hash";
		}

		public static string GenerateHashName()
		{
			return "good_hash";
		}
	}
}
