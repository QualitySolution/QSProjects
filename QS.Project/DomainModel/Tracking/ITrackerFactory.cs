using System;

namespace QS.DomainModel.Tracking
{
	public interface ITrackerFactory
	{
		IHibernateTracker CreateHibernateTracker();

		bool NeedTrace(Type type);
	}
}
