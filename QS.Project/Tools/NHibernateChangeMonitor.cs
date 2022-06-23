using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Tools {
	public class NHibernateChangeMonitor : IChangeMonitor
	{
		public void SubscribeAllChange<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject {
			SubscribeToCreate(criteria);
			SubscribeToDelete(criteria);
			SubscribeToUpdates(criteria);
		}

		public void SubscribeToDelete<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject {
			throw new NotImplementedException();
		}

		public void SubscribeToUpdates<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject {
			throw new NotImplementedException();
		}

		public void SubscribeToCreate<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject {
			throw new NotImplementedException();
		}

		public HashSet<int> EntityIds {
			get {
				var allIds = new HashSet<int>(IdsCreateEntities);
				allIds.UnionWith(IdsDeletedEntities);
				allIds.UnionWith(IdsUpdateEntities);
				return allIds;
			}
		}

		public HashSet<int> IdsDeletedEntities { get; }
		public HashSet<int> IdsUpdateEntities { get; }
		public HashSet<int> IdsCreateEntities { get; }

		private Func<IDomainObject, bool> criteriaSubscribeToDeletion;
		private Func<IDomainObject, bool> criteriaSubscribeToDeletion;
		private Func<IDomainObject, bool> criteriaSubscribeToDeletion;
	}
}
