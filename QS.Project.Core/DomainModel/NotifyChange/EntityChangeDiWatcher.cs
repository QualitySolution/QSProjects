using System;
using System.Collections.Generic;
using QS.DomainModel.NotifyChange.Conditions;

namespace QS.DomainModel.NotifyChange {
	/// <summary>
	/// Класс с отслеживанием изменений сущностей, предназначенный для использования с DI
	/// В отличии от глобального инстанса <see cref="NotifyConfiguration.Instance"/> не требует явного отписывания.
	/// Так как реализует интерфейс <see cref="IDisposable"/>, он будет отписан автоматически при уничтожении c DI контейнером.
	/// </summary>
	public class EntityChangeDiWatcher : IEntityChangeWatcher, IDisposable {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IEntityChangeWatcher globalWatcher;

		private HashSet<object> toDispose = new HashSet<object>();

		public EntityChangeDiWatcher(IEntityChangeWatcher globalWatcher) {
			this.globalWatcher = globalWatcher ?? throw new ArgumentNullException(nameof(globalWatcher));
		}

		public void BatchSubscribeOnAll(BatchEntityChangeHandler subscriber) {
			globalWatcher.BatchSubscribeOnAll(subscriber);
			toDispose.Add(subscriber.Target);
		}

		public void BatchSubscribeOnEntity(BatchEntityChangeHandler subscriber, params Type[] entityClasses) {
			globalWatcher.BatchSubscribeOnEntity(subscriber, entityClasses);
			toDispose.Add(subscriber.Target);
		}

		public void BatchSubscribeOnEntity<TEntity>(BatchEntityChangeHandler subscriber) {
			globalWatcher.BatchSubscribeOnEntity<TEntity>(subscriber);
			toDispose.Add(subscriber.Target);
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2>(BatchEntityChangeHandler subscriber) {
			globalWatcher.BatchSubscribeOnEntity<TEntity1, TEntity2>(subscriber);
			toDispose.Add(subscriber.Target);
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3>(BatchEntityChangeHandler subscriber) {
			globalWatcher.BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3>(subscriber);
			toDispose.Add(subscriber.Target);
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3, TEntity4>(BatchEntityChangeHandler subscriber) {
			globalWatcher.BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3, TEntity4>(subscriber);
			toDispose.Add(subscriber.Target);
		}

		public SelectionConditions BatchSubscribe(BatchEntityChangeHandler subscriber) {
			toDispose.Add(subscriber.Target);
			return globalWatcher.BatchSubscribe(subscriber);
		}

		public void UnsubscribeAll(object owner) {
			logger.Warn($"Для {nameof(EntityChangeDiWatcher)} не требуется явного отписывания. Отписка произойдет через вызов {nameof(Dispose)}().");
			toDispose.Remove(owner);
			globalWatcher.UnsubscribeAll(owner);
		}

		public void Dispose() {
			foreach(var target in toDispose) {
				globalWatcher.UnsubscribeAll(target);
			}
		}
	}
}
