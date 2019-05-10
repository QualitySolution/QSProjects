using System;
namespace QS.DomainModel.NotifyChange
{
	public interface IEntityChangeWatcher
	{
		void WatchOne<TEntity>(SingleEntityChangeEventMethod subscriber);
		void WatchMany<TEntity>(ManyEntityChangeEventMethod subscriber);
		void WatchMany<TEntity1, TEntity2>(ManyEntityChangeEventMethod subscriber);
		void WatchMany<TEntity1, TEntity2, TEntity3>(ManyEntityChangeEventMethod subscriber);
		void WatchMany<TEntity1, TEntity2, TEntity3, TEntity4>(ManyEntityChangeEventMethod subscriber);

		void UnsubscribeAll(object owner);
		void Unsubscribe(SingleEntityChangeEventMethod subscriber);
		void Unsubscribe(ManyEntityChangeEventMethod subscriber);
	}
}
