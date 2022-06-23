using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Tools {
	public interface IChangeMonitor
	{
		void SubscribeAllChange<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) where TDomainObject : class, IDomainObject;

		void SubscribeToDelete<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) where TDomainObject : class, IDomainObject;

		void SubscribeToUpdates<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) where TDomainObject : class, IDomainObject;

		void SubscribeToCreate<TDomainObject>(Func<TDomainObject, bool> criteria, IUnitOfWork unitOfWork) where TDomainObject : class, IDomainObject;
		
		HashSet<int> EntityIds { get; }
		
		HashSet<int> IdsDeletedEntities { get; }
		
		HashSet<int> IdsUpdateEntities { get; }
		
		HashSet<int> IdsCreateEntities { get; }
	}
}
