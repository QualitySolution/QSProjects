using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Tools {
	public interface IChangeMonitor
	{
		void SubscribeAllChange<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject;

		void SubscribeToDelete<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject;

		void SubscribeToUpdates<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject;

		void SubscribeToCreate<TDomainObject>(Func<TDomainObject, bool> criteria) where TDomainObject : IDomainObject;
		
		HashSet<int> EntityIds { get; }
		
		HashSet<int> IdsDeletedEntities { get; }
		
		HashSet<int> IdsUpdateEntities { get; }
		
		HashSet<int> IdsCreateEntities { get; }
	}
}
