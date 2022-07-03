using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;

namespace QS.Tools {
	/// <summary>
	/// Сервис для отслеживания изменений полей при сохранении сущностей
	/// </summary>
	public class NHibernateChangeMonitor<TDomainObject> : IChangeMonitor<TDomainObject>, IDisposable
	where TDomainObject : class, IDomainObject 
	{
		public IIChangeConfiguration<TDomainObject> SubscribeAllChange(
			Func<TDomainObject, bool> criteria) 
		{
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToCreate();
			SubscribeToDelete();
			SubscribeToUpdates();
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}
		
		public IIChangeConfiguration<TDomainObject> SubscribeToDelete(
			Func<TDomainObject, bool> criteria) 
		{
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToDelete();
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}

		public IIChangeConfiguration<TDomainObject> SubscribeToUpdates(
			Func<TDomainObject, bool> criteria) 
		{
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToUpdates();
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}
		
		public IIChangeConfiguration<TDomainObject> SubscribeToCreate(
			Func<TDomainObject, bool> criteria) 
		{
			Criteria = o => criteria((TDomainObject)o);
			SubscribeToCreate();
			return new ChangeConfiguration<TDomainObject>(SetTargetField);
		}

		public void AddSetTargetUnitOfWorks(IUnitOfWork unitOfWork) {
			TargetUnitOfWorks.Add(unitOfWork);
		}

		private void SubscribeToDelete() {
			NotifyConfiguration.Instance.BatchSubscribe(Delete)
				.IfEntity<TDomainObject>()
				.AndWhere(Criteria)
				.AndChangeType(TypeOfChangeEvent.Delete);
		}

		private void SubscribeToUpdates()  {
			NotifyConfiguration.Instance.BatchSubscribe(Update)
				.IfEntity<TDomainObject>()
				.AndWhere(Criteria)
				.AndChangeType(TypeOfChangeEvent.Update);
		}
		
		private void SubscribeToCreate()  {
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
		private List<IUnitOfWork> TargetUnitOfWorks { get; } = new List<IUnitOfWork>();
		private Func<IDomainObject, bool> Criteria { get; set; }

		private void Delete(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsDeletedEntities);

		private void Insert(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsCreateEntities);

		private void Update(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsUpdateEntities);

		private void SaveIds(EntityChangeEvent[] changeEvents, HashSet<int> ids) {
			foreach(var changeEvent in changeEvents)
				if(TargetUnitOfWorks.Any()) {
					if(TargetUnitOfWorks.Any(x => x.Session == changeEvent.Session))
						ids.Add(targetField is null
							? changeEvent.Entity.GetId()
							: targetField(changeEvent.Entity).GetId());
				}
				else
					ids.Add(targetField is null
						? changeEvent.Entity.GetId()
						: targetField(changeEvent.Entity).GetId());
		}
		
		private void SetTargetField<TTarget>(Func<TDomainObject, TTarget> field) 
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
