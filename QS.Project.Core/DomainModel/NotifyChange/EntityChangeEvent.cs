using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Gamma.Utilities;
using NHibernate.Event;

[assembly:InternalsVisibleTo("QS.LibsTest.Core")]
namespace QS.DomainModel.NotifyChange
{
	public class EntityChangeEvent
	{
		/// <summary>
		/// Желательно не использовать во внешнем коде напрямую. Так как при реализации меж программных уведомлений, доступа к кассу события NHibernate не будет.
		/// </summary>
		public PostInsertEvent InsertEvent { get; private set;}
		/// <summary>
		/// Желательно не использовать во внешнем коде напрямую. Так как при реализации меж программных уведомлений, доступа к кассу события NHibernate не будет.
		/// </summary>
		public PostUpdateEvent UpdateEvent { get; private set;}
		/// <summary>
		/// Желательно не использовать во внешнем коде напрямую. Так как при реализации меж программных уведомлений, доступа к кассу события NHibernate не будет.
		/// </summary>
		public PostDeleteEvent DeleteEvent { get; private set;}

		public Type EntityClass { get; private set; }
		public object Entity { get; private set; }

		#region Конструкторы

		public EntityChangeEvent(PostInsertEvent insertEvent)
		{
			InsertEvent = insertEvent;
			UpdateEvent = null;
			DeleteEvent = null;
			EntityClass = InsertEvent.Persister.MappedClass;
			Entity = InsertEvent.Entity;
			Session = InsertEvent.Session;
			EventType = TypeOfChangeEvent.Insert;
		}

		public EntityChangeEvent(PostUpdateEvent updateEvent)
		{
			InsertEvent = null;
			UpdateEvent = updateEvent;
			DeleteEvent = null;
			EntityClass = UpdateEvent.Persister.MappedClass;
			Entity = UpdateEvent.Entity;
			Session = UpdateEvent.Session;
			EventType = TypeOfChangeEvent.Update;
		}

		public EntityChangeEvent(PostDeleteEvent deleteEvent)
		{
			InsertEvent = null;
			UpdateEvent = null;
			DeleteEvent = deleteEvent;
			EntityClass = DeleteEvent.Persister.MappedClass;
			Entity = DeleteEvent.Entity;
			Session = DeleteEvent.Session;
			EventType = TypeOfChangeEvent.Delete;
		}

		/// <summary>
		/// Используется только для тестов
		/// </summary>
		internal EntityChangeEvent(TypeOfChangeEvent typeOfChange, Type entityClass = null, object entity = null, IEventSource session = null)
		{
			EventType = typeOfChange;
			EntityClass = entityClass;
			Entity = entity;
			Session = session;
		}
		#endregion 

		public IEventSource Session { get; }

		#region Доступ к данным

		public TEntity GetEntity<TEntity>() where TEntity : class
		{
			return Entity as TEntity;
		}

		public TypeOfChangeEvent EventType { get; }

		public AbstractPostDatabaseOperationEvent PostDatabaseOperationEvent => 
			(UpdateEvent as AbstractPostDatabaseOperationEvent)
		 	?? (InsertEvent as AbstractPostDatabaseOperationEvent) 
			?? (DeleteEvent as AbstractPostDatabaseOperationEvent);

		public TResult GetOldValueCast<TEntity, TResult>(Expression<Func<TEntity, TResult>> propertyRefExpr) => (TResult)GetOldValue(PropertyUtil.GetName(propertyRefExpr));

		public object GetOldValue<TEntity>(Expression<Func<TEntity, object>> propertyRefExpr) => GetOldValue(PropertyUtil.GetName(propertyRefExpr));

		public object GetOldValue(string propertyName)
		{
			switch(EventType) {
				case TypeOfChangeEvent.Update:
					return UpdateEvent.OldState[Array.IndexOf(UpdateEvent.Persister.PropertyNames, propertyName)];
				case TypeOfChangeEvent.Insert:
					return null;
				case TypeOfChangeEvent.Delete:
					return DeleteEvent.DeletedState[Array.IndexOf(DeleteEvent.Persister.PropertyNames, propertyName)];
				default:
					throw new NotImplementedException();
			}
		}

		public TResult GetNewValueCast<TEntity, TResult>(Expression<Func<TEntity, TResult>> propertyRefExpr) => (TResult)GetNewValue(PropertyUtil.GetName(propertyRefExpr));

		public object GetNewValue<TEntity>(Expression<Func<TEntity, object>> propertyRefExpr) => GetNewValue(PropertyUtil.GetName(propertyRefExpr));

		public object GetNewValue(string propertyName)
		{
			switch(EventType) {
				case TypeOfChangeEvent.Update:
					return UpdateEvent.State[Array.IndexOf(UpdateEvent.Persister.PropertyNames, propertyName)];
				case TypeOfChangeEvent.Insert:
					return InsertEvent.State[Array.IndexOf(InsertEvent.Persister.PropertyNames, propertyName)]; ;
				case TypeOfChangeEvent.Delete:
					return null;
				default:
					throw new NotImplementedException();
			}
		}

		#endregion
	}

	public enum TypeOfChangeEvent
	{
		Insert,
		Update,
		Delete
	}
}
