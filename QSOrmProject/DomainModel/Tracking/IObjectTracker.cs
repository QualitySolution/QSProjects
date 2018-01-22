using System;

namespace QSOrmProject.DomainModel.Tracking
{
	public interface IObjectTracker<TEntity>
	{
		void TakeFirst(TEntity subject);

		bool Compare(TEntity lastSubject);
		void SaveChangeSet(IUnitOfWork uow);
	}
}
