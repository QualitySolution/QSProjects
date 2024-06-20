using NHibernate.Event;

namespace QS.DomainModel.Tracking
{
	public interface ISingleUowEventListener
	{

	}

	public interface IUowPreLoadEventListener
	{
		void OnPreLoad(IUnitOfWorkTracked uow, PreLoadEvent loadEvent);
	}

	public interface IUowPostLoadEventListener
	{
		void OnPostLoad(IUnitOfWorkTracked uow, PostLoadEvent loadEvent);
	}

	public interface IUowPostInsertEventListener
	{
		void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent);
	}

	public interface IUowPostUpdateEventListener
	{
		void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent);
	}

	public interface IUowPostDeleteEventListener
	{
		void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent);
	}

	public interface IUowPostCommitEventListener
	{
		void OnPostCommit(IUnitOfWorkTracked uow);
	}
}
