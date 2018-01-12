using System;

namespace QSOrmProject.DomainModel.Tracking
{
	public interface IObjectTracker<TEntity>
	{
		bool Compare(TEntity lastSubject);
	}
}
