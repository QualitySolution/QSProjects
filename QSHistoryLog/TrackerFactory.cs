using System;
using QSOrmProject;
using QSOrmProject.DomainModel.Tracking;

namespace QSHistoryLog
{
	public class TrackerFactory : ITrackerFactory
	{
		public TrackerFactory()
		{
		}

		public IObjectTracker<TEntity> Create<TEntity>(TEntity root, bool isNew)
			where TEntity : class, IDomainObject, new()
		{
			return new ObjectTracker<TEntity>(root, isNew);
		}
	}
}
