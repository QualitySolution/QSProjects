using System;
using System.Linq;
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
			if(HistoryMain.ObjectsDesc.Any(x => x.ObjectType == typeof(TEntity)))
				return new ObjectTracker<TEntity>(root, isNew);
			else
				return null;
		}
	}
}
