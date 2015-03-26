using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Reflection;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using QSOrmProject;

namespace QSHistoryLog
{
	public static class HistoryMain
	{
		public static List<HistoryObjectDesc> ObjectsDesc = new List<HistoryObjectDesc> ();
		const string FieldNameSeparator = ".";

		static CompareLogic qsCompareLogic;

		public static CompareLogic QSCompareLogic {
			get {
				if (qsCompareLogic == null) {
					qsCompareLogic = createQSCompareLogic ();
				}
				return qsCompareLogic;
			}
		}

		static CompareLogic createQSCompareLogic ()
		{
			var logic = new CompareLogic ();
			logic.Config.CompareReadOnly = false;
			logic.Config.CompareStaticFields = false;
			logic.Config.CompareStaticProperties = false;
			logic.Config.MaxDifferences = 10000;
			logic.Config.CustomComparers.Add (new DomainObjectComparer(RootComparerFactory.GetRootComparer()));

			return logic;
		}

		public static void AddClass (Type type)
		{
			ObjectsDesc.Add (new HistoryObjectDesc (type));
		}

		public static string ResolveFieldNameFromPath (string path, bool cutClass = true)
		{
			string result = String.Empty;
			string[] parts = path.Split ('.');
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
			var field = parentClass.GetProperty (path [0]);
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

		internal static string GetObjectTilte(object value)
		{
			var prop = value.GetType ().GetProperty ("Title");
			if (prop != null) {
				return String.Format ("[{0}]", prop.GetValue (value, null));
			}

			prop = value.GetType ().GetProperty ("Name");
			if (prop != null) {
				return String.Format ("[{0}]", prop.GetValue (value, null));
			}

			return value.ToString ();
		}
	}

	public enum ChangeSetType
	{
		[ItemTitle ("Создание")]
		Create,
		[ItemTitle ("Изменение")]
		Change,
		[ItemTitle ("Удаление")]
		Delete
	}

	public enum FieldChangeType
	{
		[ItemTitle ("Добавлено")]
		Added,
		[ItemTitle ("Изменено")]
		Changed,
		[ItemTitle ("Удалено")]
		Removed,
		[ItemTitle ("Без изменений")]
		Unchanged
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

