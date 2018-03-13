using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Type;
using QSOrmProject.DomainModel.Tracking;

namespace QSOrmProject.DomainModel
{
	internal class UowSessionInterceptor : EmptyInterceptor
	{
		List<IObjectTracker> trackers;
		IUnitOfWork AttachedUow;

		public UowSessionInterceptor(IUnitOfWork attachedUow)
		{
			AttachedUow = attachedUow;
		}

        public override bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
			if(entity is IBusinessObject)
			{
				(entity as IBusinessObject).UoW = AttachedUow;
			}
			return base.OnLoad(entity, id, state, propertyNames, types);
        }
    }
}
