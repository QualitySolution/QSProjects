using System;
namespace QS.DomainModel.NotifyChange
{
	public interface IEntityChangeWatcher
	{
		void SingleSubscribeOnEntity<TEntity>(SingleEntityChangeEventMethod subscriber);
		void BatchSubscribeOnAll(BatchEntityChangeHandler subscriber);
		void BatchSubscribeOnEntity(BatchEntityChangeHandler subscriber, params Type[] entityClasses);
		void BatchSubscribeOnEntity<TEntity>(BatchEntityChangeHandler subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2>(BatchEntityChangeHandler subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3>(BatchEntityChangeHandler subscriber);
		void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3, TEntity4>(BatchEntityChangeHandler subscriber);

		Conditions.SelectionConditions BatchSubscribe(BatchEntityChangeHandler subscriber);

		void UnsubscribeAll(object owner);
		void Unsubscribe(SingleEntityChangeEventMethod subscriber);
		void Unsubscribe(BatchEntityChangeHandler subscriber);
	}
}
