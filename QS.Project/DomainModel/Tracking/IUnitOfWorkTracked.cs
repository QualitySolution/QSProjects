using NHibernate;
using NHibernate.Event;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Tracking
{
	public interface IUnitOfWorkTracked
    {
        ISession Session { get; }
        UnitOfWorkTitle ActionTitle { get; }

		IHibernateTracker HibernateTracker { get; }

		void OnPostLoad(PostLoadEvent loadEvent);
		void OnPostDelete(PostDeleteEvent deleteEvent);
	}
}
