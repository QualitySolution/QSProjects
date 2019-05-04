using System;
using QS.DomainModel.Tracking;

namespace QS.HistoryLog
{
	public class TrackerFactory : ISingleUowEventsListnerFactory
	{
		public TrackerFactory()
		{
		}

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
		{
			return new HibernateTracker();
		}
	}
}
