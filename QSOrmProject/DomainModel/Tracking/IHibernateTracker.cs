using System;
using NHibernate.Event;
using QSOrmProject;

namespace QS.DomainModel.Tracking
{
	public interface IHibernateTracker
	{
		void OnPostUpdate(PostUpdateEvent updateEvent);
		void OnPostDelete(PostDeleteEvent deleteEvent);
		void Reset();

		void SaveChangeSet();
	}
}
