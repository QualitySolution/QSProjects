using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using QS.DomainModel.Tracking;
using QSHistoryLog;
using QSOrmProject;

namespace QS.HistoryLog
{
	public static class HistoryMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc> ();
		const string FieldNameSeparator = ".";

		static HistoryMain()
		{
			TrackerMain.Factory = new TrackerFactory();
		}

		public static HistoryObjectDesc AddClass (Type type)
		{
			var desc = new HistoryObjectDesc (type);
			ObjectsDesc.Add (desc);
			return desc;
		}

		/// <summary>
		/// Читаем классы для отслеживания из OrmMain
		/// </summary>
		public static void ConfigureFromOrmMain()
		{
			foreach(var clazz in OrmMain.ClassMappingList.Where(x => x.IsTrace))
			{
				AddClass(clazz.ObjectClass);
			}

			//FIXME Отключил временно пока не релализована поддежка журналирования удаления через ORM.
			//SubscribeToDeletion();
		}

		public static string ResolveFieldNameFromPath (string path, bool cutClass = true)
		{
			string result = String.Empty;
			string[] parts = Regex.Split (path, @"\.(.*\[.*\]|.+)(?:\.|$)");

			if (parts.Length <= 0)
				return result;
			var desc = ObjectsDesc.Find (d => d.ObjectName == parts [0]);
			System.Type classType = desc != null ? desc.ObjectType : FineClass (parts [0]);

			if (desc == null && classType != null)
				desc = new HistoryObjectDesc (classType);

			if (!cutClass) {
				if (desc != null)
					result = desc.DisplayName + FieldNameSeparator;
				else
					result = parts [0] + FieldNameSeparator;
			}
				
			if (classType == null)
				for (int i = 1; i < parts.Length; i++)
					result += parts [i] + FieldNameSeparator;
			else if (parts.Length > 1) {
				result += ResolveFieldName (classType, parts.Where ((val, idx) => idx != 0).ToArray ());
			}
					
			return result.TrimEnd (FieldNameSeparator.ToCharArray ());
		}

		private static Type FineClass (string className)
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (Type t in a.GetTypes()) {
					if (t.Name == className)
						return t;
				}
			}
			return null;
		}

		private static string ResolveFieldName (Type parentClass, string[] path)
		{
			string prop, key = String.Empty;
			var reg = Regex.Match (path [0], @"^(.*)\[(.*)\]$");
			if (reg.Success) {
				prop = reg.Groups [1].Value;
				key = reg.Groups [2].Value;
			} else {
				prop = path [0];
			}

			var field = parentClass.GetProperty (prop);
			if (field == null)
				return String.Join (FieldNameSeparator, path);
				
			var att = field.GetCustomAttributes (typeof(DisplayAttribute), false);
			string name = att.Length > 0 ? (att [0] as DisplayAttribute).GetName () : path [0];

			if(!String.IsNullOrEmpty (key))
			{
				var desc = ObjectsDesc.Find (d => d.ObjectType == parentClass);
				if(desc != null)
				{
					if(desc.PropertiesKeyTitleFunc.ContainsKey (prop))
					{
						var title = desc.PropertiesKeyTitleFunc[prop] (key);
						if (!String.IsNullOrEmpty (title))
							key = title;
					}
				}
				name += String.Format ("[{0}]", key);
			}

			if (path.Length > 1) {
				string recusiveFieldName;
				if(field.PropertyType.IsGenericType && field.PropertyType.GetGenericTypeDefinition () == typeof(List<>))
					recusiveFieldName = ResolveFieldName (field.PropertyType.GetGenericArguments ()[0], path.Where ((val, idx) => idx != 0).ToArray ());
				else
					recusiveFieldName =	ResolveFieldName (field.PropertyType, path.Where ((val, idx) => idx != 0).ToArray ());
				return name + FieldNameSeparator + recusiveFieldName;
			}
			else
				return name + FieldNameSeparator;
		}

		internal static string GetObjectTilte(object value)
		{
			return String.Format ("[{0}]", QSOrmProject.DomainHelper.GetObjectTilte(value));
		}

		internal static int? GetObjectId(object value)
		{
			var prop = value.GetType ().GetProperty ("Id");
			return prop != null ? (int?)prop.GetValue (value, null) : null;
		}
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

