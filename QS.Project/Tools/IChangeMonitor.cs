using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Tools {
	public interface IChangeMonitor
	{
		IIChangeConfiguration<TDomainObject> SubscribeAllChange<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject;

		IIChangeConfiguration<TDomainObject> SubscribeToDelete<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject;

		IIChangeConfiguration<TDomainObject> SubscribeToUpdates<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject;

		IIChangeConfiguration<TDomainObject> SubscribeToCreate<TDomainObject>(
			Func<TDomainObject, bool> criteria, 
			IUnitOfWork unitOfWork) 
			where TDomainObject : class, IDomainObject;
		
		HashSet<int> EntityIds { get; }
		HashSet<int> IdsDeletedEntities { get; }
		HashSet<int> IdsUpdateEntities { get; }
		HashSet<int> IdsCreateEntities { get; }
	}

	// ReSharper disable once TypeParameterCanBeVariant
	public interface IIChangeConfiguration<TDomainObject> {
		void TargetField<TTarget>(Func<TDomainObject, TTarget> targetField)
			where TTarget : IDomainObject;
	}
}
