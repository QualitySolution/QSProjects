using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public static string ResolveFieldTilte(string clazz, string fieldName)
		{
			var desc = ObjectsDesc.FirstOrDefault(x => x.ObjectName == clazz);
			if(desc != null) {
				return DomainHelper.GetPropertyTitle(desc.ObjectType, fieldName) ?? fieldName;
			}
			return fieldName;
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

