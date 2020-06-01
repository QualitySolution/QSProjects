using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.ViewModels.Control.EEVM
{
	public class UowEntityAdapter<TEntity> : IEntityAdapter<TEntity>, IDisposable
		where TEntity : IDomainObject
	{
		private readonly IUnitOfWork uow;

		TEntity entity;

		public UowEntityAdapter(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));

			DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribeOnEntity(ExternalEntityChangeEventMethod, typeof(TEntity));
		}

		public void Dispose()
		{
			DomainModel.NotifyChange.NotifyConfiguration.Instance.UnsubscribeAll(this);
		}

		public TEntity GetEntityByNode(object node)
		{
			entity = uow.GetById<TEntity>(node.GetId());
			return entity;
		}

		void ExternalEntityChangeEventMethod(DomainModel.NotifyChange.EntityChangeEvent[] changeEvents)
		{
			object foundUpdatedObject = changeEvents.FirstOrDefault(e => DomainHelper.EqualDomainObjects(e.Entity, entity));
			if(foundUpdatedObject != null && uow.Session.IsOpen && uow.Session.Contains(entity)) {
				uow.Session.Refresh(entity);
			}
		}
	}
}
