using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.Project.DB;

namespace QS.HistoryLog
{
	public static class HistoryMain
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static void Enable(MySqlConnectionStringBuilder connectionFactory)
		{
			SingleUowEventsTracker.RegisterSingleUowListnerFactory(new TrackerFactory(connectionFactory));
		}

		public static IEnumerable<HistoryObjectDesc> TraceClasses {
			get {
				return OrmConfig.NhConfig.ClassMappings
					.Where(x => x.MappedClass.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length > 0)
					.Select(x => new HistoryObjectDesc(x.MappedClass));
			}
		}

		internal static Type FineEntityClass (string className)
		{
			return OrmConfig.NhConfig.ClassMappings
					.Where(x => x.MappedClass.Name == className)
					.Select(x => x.MappedClass)
					.FirstOrDefault();
		}

		public static string ResolveFieldTitle(string clazz, string fieldName)
		{
			var type = FineEntityClass(clazz);
			if(type != null) {
				return DomainHelper.GetPropertyTitle(type, fieldName) ?? fieldName;
			}
			return fieldName;
		}

		internal static string GetObjectTitle(object value)
		{
			return value == null ? null : String.Format ("[{0}]", DomainHelper.GetTitle(value));
		}
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

