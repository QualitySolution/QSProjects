using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings;
using System.Reflection;
using System.Linq;

namespace QSHistoryLog
{
	public static class HistoryMain
	{
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc>();
		const string FieldNameSeparator = ".";

		public static void AddClass(Type type)
		{
			ObjectsDesc.Add (new HistoryObjectDesc (type));
		}

		public static string ResolveFieldNameFromPath(string path, bool cutClass = true)
		{
			string result = String.Empty;
			string[] parts = path.Split ('.');
			if (parts.Length <= 0)
				return result;
			var desc = ObjectsDesc.Find (d => d.ObjectName == parts [0]);
			System.Type classType = desc != null ? desc.ObjectType : FineClass (parts [0]);

			if (desc == null && classType != null)
				desc = new HistoryObjectDesc (classType);

			if(!cutClass)
			{
				if (desc != null)
					result = desc.DisplayName + FieldNameSeparator;
				else
					result = parts [0] + FieldNameSeparator;
			}
				
			if(classType == null)
				for(int i = 1; i < parts.Length; i++)
					result += parts [i] + FieldNameSeparator;
			else if(parts.Length > 1)
			{
				result += ResolveFieldName (classType, parts.Where ((val, idx) => idx != 0).ToArray ());
			}
					
			return result.TrimEnd (FieldNameSeparator.ToCharArray ());
		}

		private static Type FineClass(string className)
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetTypes())
				{
					if (t.Name == className)
						return t;
				}
			}
			return null;
		}

		private static string ResolveFieldName(Type parentClass, string[] path)
		{
			var field = parentClass.GetProperty (path[0]);
			if (field == null)
				return String.Join (FieldNameSeparator, path);
				
			var att = field.GetCustomAttributes (typeof(DisplayAttribute), false);
			string name = att.Length > 0 ? (att [0] as DisplayAttribute).GetName () : path [0];

			if (path.Length > 1)
				return name + FieldNameSeparator + 
					ResolveFieldName (field.PropertyType, path.Where ((val, idx) => idx != 0).ToArray ());
			else
				return name + FieldNameSeparator;
		}
	}

	public enum ChangeSetType{
		[ItemTitle("Создание")]
		Create,
		[ItemTitle("Изменение")]
		Change,
		[ItemTitle("Удаление")]
		Delete
	}
}

