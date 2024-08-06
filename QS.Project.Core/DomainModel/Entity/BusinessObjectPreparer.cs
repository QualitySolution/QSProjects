using System;
using NHibernate.Event;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Entity
{
	public class BusinessObjectPreparer : IUowPreLoadEventListener, IUowPostInsertEventListener {
		private static BusinessObjectPreparer instance;

		public static void Init()
		{
			if(instance != null)
				return;
			//Подписываемся на события глобально
			instance = new BusinessObjectPreparer();
			GlobalUowEventsTracker.RegisterGlobalListener(instance);
		}

		private BusinessObjectPreparer() 
		{

		}

		public void OnPreLoad(IUnitOfWorkTracked uow, PreLoadEvent loadEvent)
		{
			if (loadEvent.Entity is IBusinessObject)
			{
				(loadEvent.Entity as IBusinessObject).UoW = (IUnitOfWork)uow;
			}
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			if(insertEvent.Entity is IBusinessObject && (insertEvent.Entity as IBusinessObject).UoW == null) {
				(insertEvent.Entity as IBusinessObject).UoW = (IUnitOfWork)uow;
			}
		}
	}
}
