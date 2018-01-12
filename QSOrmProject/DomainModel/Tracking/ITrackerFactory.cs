using System;
namespace QSOrmProject.DomainModel.Tracking
{
	public interface ITrackerFactory
	{
		IObjectTracker<TEntity> Create<TEntity>(TEntity root, bool isNew) where TEntity : class, IDomainObject, new();
	}
}
