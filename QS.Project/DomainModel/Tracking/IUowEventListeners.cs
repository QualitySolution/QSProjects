using NHibernate.Event;

namespace QS.DomainModel.Tracking
{
	public interface IUowPreLoadEventListener
	{
		void OnPreLoad(IUnitOfWorkTracked uow, PreLoadEvent loadEvent);
	}
}