using System;
using NHibernate.Event;

namespace QS.DomainModel.NotifyChange
{
	public class EntityChangeEvent
	{
		public PostInsertEvent InsertEvent { get; private set;}
		public PostUpdateEvent UpdateEvent { get; private set; }

		public Type EntityClass { get; private set; }
		public object Entity { get; private set; }

		public TEntity GetEntity<TEntity>() where TEntity : class
		{
			return Entity as TEntity;
		}

		public EntityChangeEvent(PostInsertEvent insertEvent)
		{
			InsertEvent = insertEvent;
			UpdateEvent = null;
			EntityClass = InsertEvent.Persister.MappedClass;
			Entity = InsertEvent.Entity;
		}

		public EntityChangeEvent(PostUpdateEvent updateEvent)
		{
			InsertEvent = null;
			UpdateEvent = updateEvent;
			EntityClass = UpdateEvent.Persister.MappedClass;
			Entity = UpdateEvent.Entity;
		}
	}
}
