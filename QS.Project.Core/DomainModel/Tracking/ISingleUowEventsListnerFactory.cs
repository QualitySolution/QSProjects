using System;

namespace QS.DomainModel.Tracking
{
	public interface ISingleUowEventsListnerFactory
	{
		ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow);
	}
}
