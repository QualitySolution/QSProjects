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
			var hash = TabHashHelper.GetTabHash(typeof(Tab_NotGetGeneric_TestClass), new Type[] { }, new object[] { });
			Assert.That(hash, Is.EqualTo("good_hash"));
		}

		[Test(Description = "Проверяем что когда ищем методы получения хеша то корректно учитываем аргументы метода.")]
		public void GetTabHash_UseParametersTypesCase()
		{
			var hash = TabHashHelper.GetTabHash(typeof(Tab_UseParametersTypes_TestClass), 
				new Type[] {typeof(string), typeof(int) },
				new object[] {"str", 5 });
			Assert.That(hash, Is.EqualTo("good_hash"));
		}
	}

	public class Tab_NotGetGeneric_TestClass : TdiTabBase
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

	public class Tab_UseParametersTypes_TestClass : TdiTabBase
	{
		public static string GenerateHashName()
		{
			return "bad_hash";
		}

		public static string GenerateHashName(int id)
		{
			return "bad_hash";
		}

		public static string GenerateHashName(int id, string str)
		{
			return "bad_hash";
		}

		public static string GenerateHashName(string str, int id)
		{
			return "good_hash";
		}
	}
}
