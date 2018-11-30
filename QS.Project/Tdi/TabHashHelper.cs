using System;
using System.Linq;
using System.Reflection;

namespace QS.Tdi
{
	internal static class TabHashHelper
	{
		public static readonly string GenerateTabHashMethodName = "GenerateHashName";

		public static ITdiTab OpenTabSelfCreateTab(ITdiTabParent tabParent, Type tabClass, Type[] argsTypes, object[] argsValues, ITdiTab afterTab)
		{
			var getHashMethod = FindGetHashMethod(tabClass, argsTypes);
			if(getHashMethod == null) {
				var argsText = String.Join(", ", argsTypes.Select((t, i) => $"{t.Name} arg{i + 1}"));
				throw new InvalidCastException($"Для работы метода OpenTab у класа {tabClass.Name} должен быть статический метод {GenerateTabHashMethodName}({argsText})");
			}

			string hash = (string)getHashMethod.Invoke(null, argsValues);

			return tabParent.OpenTab(hash, () => (ITdiTab)Activator.CreateInstance(tabClass, argsValues), afterTab);
		}

		private static MethodInfo FindGetHashMethod(Type type, Type[] types)
		{
			var info = type.GetMethod(GenerateTabHashMethodName, types);
			if(info == null && type.BaseType != null)
				return FindGetHashMethod(type.BaseType, types);
			return info;
		}
	}
}
