using System;
using NHibernate;
using NHibernate.Event;
using QS.DomainModel.Tracking;

namespace QS.DomainModel
{
    internal interface IUnitOfWorkEventHandler
    {
        ISession Session { get; }
        UnitOfWorkTitle ActionTitle { get; }

		IHibernateTracker HibernateTracker { get; }

		void OnPostLoad(PostLoadEvent loadEvent);
		void OnPreLoad(PreLoadEvent loadEvent);
		void OnPostDelete(PostDeleteEvent deleteEvent);
	}
}
