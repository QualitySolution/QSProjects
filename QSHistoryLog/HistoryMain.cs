using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Reflection;
using System.Linq;
using SIT.Components.ObjectComparer;

namespace QSHistoryLog
{
	public static class HistoryMain
	{
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc>();
		const string FieldNameSeparator = ".";

		static Context qsContext;
		public static Context QSContext {
			get {if (qsContext == null) {
					qsContext = createQSHistoryContext ();
				}
				return qsContext;
			}
		}

		static Context createQSHistoryContext()
		{
			var mainContext = new Context ();
			if (mainContext.Configuration == null)
				mainContext.Configuration = new Configuration ();
			if (mainContext.Cache == null)
				mainContext.Cache = new Cache ();
			mainContext.Configuration.CheckStopRecursionFunc = HandleMainCheckStopRecursion;
			mainContext.MetadataReader = new QSHistoryMetadataReader (mainContext);

			return mainContext;
		}

		static bool HandleMainCheckStopRecursion (Context context, object parentValue, Type parentType, object childValue, MemberInfo childPi)
		{
			if (childPi is PropertyInfo) {
				var pi = childPi as PropertyInfo;
				//Корректно сохраняем дату и время.
				if (pi.PropertyType == typeof(DateTime))
					return true;
				//Не идем в нутрь обьектов доменной модели
				if (pi.PropertyType.IsClass)
				{
					if (pi.PropertyType.GetProperty ("Title") != null 
					    || pi.PropertyType.GetProperty ("Name") != null)
						return true;
				}
			}
			return false;
		}

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

