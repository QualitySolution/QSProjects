using System;
using System.Reflection;
using System.Text;
using System.Collections;

namespace QSProjectsLib
{
	public static class DebugWorks
	{

		public static string PrintValues(object obj)
		{
			StringBuilder builder = new StringBuilder ();
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
				if (property.PropertyType.Assembly == objType.Assembly)
				{
					builder.AppendLine (String.Format ("{0}{1}:", indentString, property.Name));
					PrintProperties(builder, propValue, indent + 2);
				}
				else if(propValue is IDictionary){
					builder.AppendLine (String.Format ("{0}{1}:", indentString, property.Name));
					PrintDictionary(builder, propValue as IDictionary, objType.Assembly, indent + 2);
				}
				else
				{
					builder.AppendLine (String.Format ("{0}{1}: {2}", indentString, property.Name, propValue));
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

		public static string PrintStackTrace()
		{
			return Environment.StackTrace;
		}

	}
}

