using System;
namespace QS.DomainModel.NotifyChange
{
	public interface IEntityChangeWatcher
	{
		void SingleSubscribeOnEntity<TEntity>(SingleEntityChangeEventMethod subscriber);
		void BatchSubscribeOnAll(ManyEntityChangeEventMethod subscriber);
		void BatchSubscribeOnEntity(ManyEntityChangeEventMethod subscriber, params Type[] entityClasses);
		void BatchSubscribeOnEntity<TEntity>(ManyEntityChangeEventMethod subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2>(ManyEntityChangeEventMethod subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3>(ManyEntityChangeEventMethod subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3, TEntity4>(ManyEntityChangeEventMethod subscriber);

		void UnsubscribeAll(object owner);
		void Unsubscribe(SingleEntityChangeEventMethod subscriber);
		void Unsubscribe(ManyEntityChangeEventMethod subscriber);
	}
}
