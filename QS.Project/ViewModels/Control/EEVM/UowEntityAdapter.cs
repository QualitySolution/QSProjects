using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;

namespace QS.ViewModels.Control.EEVM
{
	public class UowEntityAdapter<TEntity> : IEntityAdapter<TEntity>, IDisposable
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork uow;
		protected readonly IEntityChangeWatcher ChangeWatcher;

		public UowEntityAdapter(IEEVMBuilderParameters parameters)
		{
			if(parameters == null) throw new ArgumentNullException(nameof(parameters));
			this.uow = parameters.UnitOfWork ?? throw new ArgumentNullException(nameof(parameters.UnitOfWork));
			ChangeWatcher = parameters.ChangeWatcher;
			ChangeWatcher?.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, typeof(TEntity));
		}

		public EntityEntryViewModel<TEntity> EntityEntryViewModel { set; get; }


		public TEntity GetEntityByNode(object node)
		{
			return uow.GetById<TEntity>(node.GetId());
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			if(EntityEntryViewModel is null) {
				return;
			}
			
			var foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, EntityEntryViewModel.Entity));
			if(foundUpdatedObject != null && uow.Session.IsOpen && uow.Session.Contains(EntityEntryViewModel.Entity)) {
				if(foundUpdatedObject.EventType == DomainModel.NotifyChange.TypeOfChangeEvent.Delete)
					EntityEntryViewModel.Entity = null;
				else
					uow.Session.Refresh(EntityEntryViewModel.Entity);
			}
		}

		public void Dispose()
		{
			ChangeWatcher?.UnsubscribeAll(this);
			EntityEntryViewModel = null;
		}
	}
}
