using System;
using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;

namespace QS.Tools {
	/// <summary>
	/// Сервис для отслеживания изменений полей при сохранении сущностей
	/// </summary>
	public class NHibernateChangeMonitor : IChangeMonitor, IDisposable
	{
		public IIChangeConfiguration<TDomainObject> SubscribeAllChange<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject 
		{
			Session = unitOfWork.Session;
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToCreate<TDomainObject>();
			SubscribeToDelete<TDomainObject>();
			SubscribeToUpdates<TDomainObject>();

			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}
		
		public IIChangeConfiguration<TDomainObject> SubscribeToDelete<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject 
		{
			Session = unitOfWork.Session;
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToDelete<TDomainObject>();
			
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}

		public IIChangeConfiguration<TDomainObject> SubscribeToUpdates<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject 
		{
			Session = unitOfWork.Session;
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToUpdates<TDomainObject>();
			
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}
		
		public IIChangeConfiguration<TDomainObject> SubscribeToCreate<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject 
		{
			Session = unitOfWork.Session;
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToCreate<TDomainObject>();
			
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}

		private void SubscribeToDelete<TDomainObject>() 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Delete)
				.IfEntity<TDomainObject>()
				.AndWhere(Criteria)
				.AndChangeType(TypeOfChangeEvent.Delete);
		}

		private void SubscribeToUpdates<TDomainObject>() 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Update)
				.IfEntity<TDomainObject>()
				.AndWhere(Criteria)
				.AndChangeType(TypeOfChangeEvent.Update);
		}
		
		private void SubscribeToCreate<TDomainObject>() 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Insert)
				.IfEntity<TDomainObject>()
				.AndWhere(Criteria)
				.AndChangeType(TypeOfChangeEvent.Insert);
		}
		
		public HashSet<int> EntityIds {
			get {
				var allIds = new HashSet<int>(IdsCreateEntities);
				allIds.UnionWith(IdsDeletedEntities);
				allIds.UnionWith(IdsUpdateEntities);
				return allIds;
			}
		}

		public HashSet<int> IdsDeletedEntities { get; } = new HashSet<int>();
		public HashSet<int> IdsUpdateEntities { get; } = new HashSet<int>();
		public HashSet<int> IdsCreateEntities { get; } = new HashSet<int>();
		private ISession Session { get; set; }
		private Func<IDomainObject, bool> Criteria { get; set; }

		private void Delete(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsDeletedEntities);

		private void Insert(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsCreateEntities);

		private void Update(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsUpdateEntities);

		private void SaveIds(EntityChangeEvent[] changeEvents, HashSet<int> ids) {
			foreach(var changeEvent in changeEvents)
				if(changeEvent.Session == Session)
					ids.Add(targetField is null ? 
						changeEvent.Entity.GetId() 
						: targetField(changeEvent.Entity).GetId());
		}
		
		private void SetTargetField<TDomainObject, TTarget>(Func<TDomainObject, TTarget> field) 
			where TTarget : IDomainObject
			=> targetField = o => field((TDomainObject)o);

		private Func<object, IDomainObject> targetField;

		public void Dispose() => NotifyConfiguration.Instance.UnsubscribeAll(this);
	}

	public class ChangeConfiguration<TDomainObject> : IIChangeConfiguration<TDomainObject> {
		public void TargetField<TTarget>(Func<TDomainObject, TTarget> targetField)
			where TTarget : IDomainObject
			=> setField(o => targetField(o));

		public ChangeConfiguration(Action<Func<TDomainObject, IDomainObject>> setField)
			=> this.setField = setField;

		private readonly Action<Func<TDomainObject,IDomainObject>> setField;
	}
}
