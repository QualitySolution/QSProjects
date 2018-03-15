using System;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface IObjectTracker<TEntity> : IObjectTracker
	{
		void TakeFirst(TEntity subject);
		TEntity OriginEntity {get;}

		bool Compare(TEntity lastSubject);
		void SaveChangeSet(IUnitOfWork uow);
	}

	public interface IObjectTracker{
		object OriginObject { get; }
		bool CompareWithOrigin();
		void SaveChangeSet(IUnitOfWork uow);
		void ResetToOrigin();
	}
}
