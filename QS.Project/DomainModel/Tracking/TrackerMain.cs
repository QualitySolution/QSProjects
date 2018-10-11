using System;
using NHibernate.Proxy;
using QS.DomainModel.Entity;

namespace QS.DomainModel.Tracking
{
	public static class TrackerMain
	{
		public static ITrackerFactory Factory;

		public static bool NeedTrace(IDomainObject entity){
			if(Factory == null)
				return false;

			return Factory.NeedTrace(NHibernateProxyHelper.GuessClass(entity));
		}

		public static bool NeedTrace(Type entityClass)
		{
			if(Factory == null)
				return false;

			if(entityClass.GetInterface(typeof(NHibernate.Proxy.DynamicProxy.IProxy).FullName) != null || entityClass.GetInterface(typeof(INHibernateProxy).FullName) != null)
				entityClass = entityClass.BaseType;
			
			return Factory.NeedTrace(entityClass);
		}
	}
}
