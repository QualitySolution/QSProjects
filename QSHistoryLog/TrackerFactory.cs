using System;
using System.Linq;
using NHibernate.Proxy;
using QS.DomainModel.Tracking;
using QSOrmProject;

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
			return HistoryMain.ObjectsDesc.Any(x => x.ObjectType == type);
		}
	}
}
