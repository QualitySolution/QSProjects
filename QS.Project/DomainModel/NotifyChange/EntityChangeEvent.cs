using System;
using NHibernate.Event;

namespace QS.DomainModel.NotifyChange
{
	public class EntityChangeEvent
	{
		public PostInsertEvent InsertEvent { get; private set;}
		public PostUpdateEvent UpdateEvent { get; private set;}
		public PostDeleteEvent DeleteEvent { get; private set;}

		public Type EntityClass { get; private set; }
		public object Entity { get; private set; }

		public TypeOfChangeEvent EventType
		{
			get {
				if (InsertEvent != null) return TypeOfChangeEvent.Insert;
				if (UpdateEvent != null) return TypeOfChangeEvent.Update;
				return TypeOfChangeEvent.Delete;
			}
		}

		public TEntity GetEntity<TEntity>() where TEntity : class
		{
			return Entity as TEntity;
		}

		public EntityChangeEvent(PostInsertEvent insertEvent)
		{
			InsertEvent = insertEvent;
			UpdateEvent = null;
			DeleteEvent = null;
			EntityClass = InsertEvent.Persister.MappedClass;
			Entity = InsertEvent.Entity;
		}

		public EntityChangeEvent(PostUpdateEvent updateEvent)
		{
			InsertEvent = null;
			UpdateEvent = updateEvent;
			DeleteEvent = null;
			EntityClass = UpdateEvent.Persister.MappedClass;
			Entity = UpdateEvent.Entity;
		}

		public EntityChangeEvent(PostDeleteEvent deleteEvent)
		{
			InsertEvent = null;
			UpdateEvent = null;
			DeleteEvent = deleteEvent;
			EntityClass = DeleteEvent.Persister.MappedClass;
			Entity = DeleteEvent.Entity;
		}
	}

	public enum TypeOfChangeEvent
	{
		Insert,
		Update,
		Delete
	}
}
