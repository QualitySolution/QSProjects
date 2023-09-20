using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
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

		public static string ResolveFieldTitle(string clazz, string fieldName)
		{
			return HistoryDomainHelper.ResolveFieldTitle(clazz, fieldName);
		}
	}

	public interface IFileTrace
	{
		string Name { set; get; }

		uint Size { set; get; }

		bool IsChanged { set; get; }
	}
}

