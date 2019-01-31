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
			var hash = GetTabHash(tabClass, argsTypes, argsValues);

			return tabParent.OpenTab(hash, () => (ITdiTab)Activator.CreateInstance(tabClass, argsValues), afterTab);
		}

		public static string GetTabHash(Type tabClass, Type[] argsTypes, object[] argsValues)
		{
			var getHashMethod = FindGetHashMethod(tabClass, argsTypes);
			if (getHashMethod == null)
			{
				var argsText = String.Join(", ", argsTypes.Select((t, i) => $"{t.Name} arg{i + 1}"));
				throw new InvalidCastException($"Для работы метода OpenTab у класа {tabClass.Name} должен быть статический метод {GenerateTabHashMethodName}({argsText})");
			}

			return (string)getHashMethod.Invoke(null, argsValues);
		}

		private static MethodInfo FindGetHashMethod(Type type, Type[] types)
		{
			var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

			var info = methods.FirstOrDefault(x => x.Name == GenerateTabHashMethodName
				&& x.IsGenericMethod == false
				&& x.ReturnType == typeof(string)
				&& CompareMethodParameters(x.GetParameters(), types)
				);
			if(info == null && type.BaseType != null)
				return FindGetHashMethod(type.BaseType, types);
			return info;
		}

		private static bool CompareMethodParameters(ParameterInfo[] parameters, Type[] types)
		{
			if (parameters.Length != types.Length)
				return false;

			return Enumerable.SequenceEqual(parameters.Select(x => x.ParameterType), types);
		}
	}
}
