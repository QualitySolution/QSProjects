using System;
using System.Reflection;
using System.Text;
using System.Collections;

namespace QS.Utilities.Debug
{
	/// <summary>
	/// Помощник упрощающий вывод дебажной информации из кода. 
	/// </summary>
	public static class DebugPrint
	{
		/// <summary>
		/// Возвращает значения всех свойств класса в иерархическом виде, то есть свойства с классами разворачиваются в глубину, на следующий уровень.
		/// Метод поддерживает работу с любыми списками реализующими IEnumerable и словарями реализующими IDictionary.
		/// Можно как подать список в качестве аргумента, так и найденные в классе списки будут развернуты.
		/// </summary>
		/// <returns>Строка для вывода на консоль или в лог.</returns>
		/// <param name="obj">Объект для обхода</param>
		public static string Values(object obj)
		{
			StringBuilder builder = new StringBuilder ();
			if(obj is IDictionary dic) {
				PrintDictionary(builder, dic, Assembly.GetCallingAssembly(), 0);
			}else if(obj is IEnumerable list) {
				PrintEnumerable(builder, list, Assembly.GetCallingAssembly(), 0);
			}
			else
				PrintProperties(builder, obj, 0);
			return builder.ToString ();
		}

		private static void PrintProperties(StringBuilder builder, object obj, int indent)
		{
			if (obj == null) return;
			string indentString = new string(' ', indent);
			Type objType = obj.GetType();
			PropertyInfo[] properties = objType.GetProperties();
			foreach (PropertyInfo property in properties)
			{
				object propValue = property.GetValue(obj, null);
				if(propValue is Enum) {
					builder.AppendLine (String.Format ("{0}{1}: {2}", indentString, property.Name, propValue));
				}
				else if (property.PropertyType.Assembly == objType.Assembly)
				{
					builder.AppendLine (String.Format ("{0}{1}:", indentString, property.Name));
					PrintProperties(builder, propValue, indent + 2);
				}
				else if(propValue is IDictionary){
					builder.AppendLine (String.Format ("{0}{1}:", indentString, property.Name));
					PrintDictionary(builder, propValue as IDictionary, objType.Assembly, indent + 2);
				} else if(propValue is IEnumerable && !(propValue is string)) {
					builder.AppendLine(String.Format("{0}{1}:", indentString, property.Name));
					PrintEnumerable(builder, propValue as IEnumerable, objType.Assembly, indent + 2);
				} else
				{
					builder.AppendLine (String.Format ("{0}{1}: {2}", indentString, property.Name, propValue ?? "null"));
				}
			}
		}

		private static void PrintDictionary(StringBuilder builder, IDictionary dic, Assembly ass, int indent)
		{
			if (dic == null) return;
			string indentString = new string(' ', indent);
			foreach (DictionaryEntry par in dic)
			{
				if (par.Value.GetType ().Assembly == ass)
				{
					builder.AppendLine (String.Format ("{0}{1}:", indentString, par.Key));
					PrintProperties(builder, par.Value, indent + 2);
				}
				else
				{
					builder.AppendLine (String.Format ("{0}{1}: {2}", indentString, par.Key, par.Value));
				}
			}
		}

		private static void PrintEnumerable(StringBuilder builder, IEnumerable list, Assembly ass, int indent)
		{
			if(list == null) return;
			string indentString = new string(' ', indent);
			int index = 0;
			foreach(var value in list) {
				if(value.GetType().Assembly == ass) {
					builder.AppendLine(String.Format("{0}{1}:", indentString, index));
					PrintProperties(builder, value, indent + 2);
				} else {
					builder.AppendLine(String.Format("{0}{1}: {2}", indentString, index, value));
				}
				index++;
			}
		}

		public static string StackTrace()
		{
			return Environment.StackTrace;
		}

	}
}

