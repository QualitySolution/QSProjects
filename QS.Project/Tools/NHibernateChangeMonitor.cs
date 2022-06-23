using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;

namespace QS.Tools {
	/// <summary>
	/// Класс создан для отслеживания изменений полей при редактировании сущностей
	/// </summary>
	public class NHibernateChangeMonitor : IChangeMonitor, IDisposable
	{
		public void SubscribeAllChange<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject {
			SubscribeToCreate(criteria, unitOfWork);
			SubscribeToDelete(criteria, unitOfWork);
			SubscribeToUpdates(criteria, unitOfWork);
		}

		public void SubscribeToDelete<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Delete)
				.IfEntity<TDomainObject>()
				.AndWhere(criteria)
				.AndChangeType(TypeOfChangeEvent.Delete);
		}

		public void SubscribeToUpdates<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Update)
				.IfEntity<TDomainObject>()
				.AndWhere(criteria)
				.AndChangeType(TypeOfChangeEvent.Update);
		}

		public void SubscribeToCreate<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject {
			NotifyConfiguration.Instance.BatchSubscribe(Insert)
				.IfEntity<TDomainObject>()
				.AndWhere(criteria)
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

		private void Delete(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsDeletedEntities);

		private void Insert(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsCreateEntities);

		private void Update(EntityChangeEvent[] changeEvents) 
			=> SaveIds(changeEvents, IdsUpdateEntities);

		private void SaveIds(EntityChangeEvent[] changeEvents, HashSet<int> ids) {
			foreach(var changeEvent in changeEvents) {
			}
		}

		public void Dispose() => NotifyConfiguration.Instance.UnsubscribeAll(this);
	}
}
