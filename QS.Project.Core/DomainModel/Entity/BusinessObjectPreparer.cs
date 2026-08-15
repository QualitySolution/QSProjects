using System.Collections.Generic;
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
			if(loadEvent.Entity is IBusinessObject businessObject) {
				businessObject.UoW = (IUnitOfWork)uow;
			}
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			if(insertEvent.Entity is IBusinessObject businessObject && businessObject.UoW == null) {
				businessObject.UoW = (IUnitOfWork)uow;
			}
		}
	}
}
