using System;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface IDeleteTracker
	{
		void MarkDeleted(IDomainObject entity);
		void Reset();

		void SaveChangeSet();
	}
}
