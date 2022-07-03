using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Tools {
	public interface IChangeMonitor<TDomainObject> 
		where TDomainObject : IDomainObject
	{
		IIChangeConfiguration<TDomainObject> SubscribeAllChange(Func<TDomainObject, bool> criteria);

		IIChangeConfiguration<TDomainObject> SubscribeToDelete(Func<TDomainObject, bool> criteria);

		IIChangeConfiguration<TDomainObject> SubscribeToUpdates(Func<TDomainObject, bool> criteria);

		IIChangeConfiguration<TDomainObject> SubscribeToCreate(Func<TDomainObject, bool> criteria);

		void AddSetTargetUnitOfWorks(IUnitOfWork unitOfWork);
		
		HashSet<int> EntityIds { get; }
		HashSet<int> IdsDeletedEntities { get; }
		HashSet<int> IdsUpdateEntities { get; }
		HashSet<int> IdsCreateEntities { get; }
	}

	// ReSharper disable once TypeParameterCanBeVariant
	public interface IIChangeConfiguration<TDomainObject> {
		void TargetField<TTarget>(Func<TDomainObject, TTarget> targetField) where TTarget : IDomainObject;
	}
}
