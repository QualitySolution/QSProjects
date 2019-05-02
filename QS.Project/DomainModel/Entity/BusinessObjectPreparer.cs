using System;
using NHibernate.Event;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;

namespace QS.DomainModel.Entity
{
	public class BusinessObjectPreparer : IUowPreLoadEventListener
	{
		private static BusinessObjectPreparer instance;

		public static void Init()
		{
			if(instance != null)
				return;
			//Подписываемся на события глобально
			instance = new BusinessObjectPreparer();
			NhEventListener.RegisterPreLoadListener(instance);
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
	}
}
