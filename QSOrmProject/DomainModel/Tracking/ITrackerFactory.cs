using System;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface ITrackerFactory
	{
		IObjectTracker<TEntity> Create<TEntity>(TEntity root, bool isNew) where TEntity : class, IDomainObject, new();
		IObjectTracker CreateTracker(object root, bool isNew);
	}
}
