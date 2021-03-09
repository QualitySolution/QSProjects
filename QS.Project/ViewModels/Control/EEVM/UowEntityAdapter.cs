using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.ViewModels.Control.EEVM
{
	public class UowEntityAdapter<TEntity> : IEntityAdapter<TEntity>, IDisposable
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork uow;

		public UowEntityAdapter(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));

			DomainModel.NotifyChange.NotifyConfiguration.Instance?.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, typeof(TEntity));
		}

		public EntityEntryViewModel<TEntity> EntityEntryViewModel { set; get; }


		public TEntity GetEntityByNode(object node)
		{
			return uow.GetById<TEntity>(node.GetId());
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			object foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, EntityEntryViewModel.Entity));
			if(foundUpdatedObject != null && uow.Session.IsOpen && uow.Session.Contains(EntityEntryViewModel.Entity)) {
				uow.Session.Refresh(EntityEntryViewModel.Entity);
			}
		}

		public void Dispose()
		{
			DomainModel.NotifyChange.NotifyConfiguration.Instance?.UnsubscribeAll(this);
		}
	}
}
