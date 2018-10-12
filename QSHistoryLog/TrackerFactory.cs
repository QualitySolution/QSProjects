using System;
using QS.DomainModel.Tracking;

namespace QS.HistoryLog
{
	public class TrackerFactory : ITrackerFactory
	{
		public TrackerFactory()
		{
		}

		public IHibernateTracker CreateHibernateTracker()
		{
			return new HibernateTracker();
		}

		public bool NeedTrace(Type type)
		{
			return type.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length > 0;
		}
	}
}
