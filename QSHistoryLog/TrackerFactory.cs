using System;
using System.Linq;
using NHibernate.Proxy;
using QS.DomainModel.Tracking;
using QSOrmProject;

namespace QSHistoryLog
{
	public class TrackerFactory : ITrackerFactory
	{
		public TrackerFactory()
		{
		}

		public IObjectTracker<TEntity> Create<TEntity>(TEntity root, bool isNew)
			where TEntity : class, IDomainObject, new()
		{
			if(HistoryMain.ObjectsDesc.Any(x => x.ObjectType == typeof(TEntity)))
				return new ObjectTracker<TEntity>(root, isNew);
			else
				return null;
		}

		public IObjectTracker CreateTracker(object root, bool isNew)
		{
			var rootType = root.GetType();
			//Здесь проверям наличие IProxy, интерфейс INHibernateProxy оставлен навсякий случай, так как в объекте с перехватом загрузки его нет.
			if(rootType.GetInterface(typeof(NHibernate.Proxy.DynamicProxy.IProxy).FullName) != null || rootType.GetInterface(typeof(INHibernateProxy).FullName) != null)
				rootType = rootType.BaseType;

			if(HistoryMain.ObjectsDesc.All(x => x.ObjectType != rootType))
				return null;

			var trackerType = typeof(ObjectTracker<>).MakeGenericType(rootType);
			return (IObjectTracker)Activator.CreateInstance(trackerType, root, isNew);
		}

	}
}
