using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using QS.DomainModel.NotifyChange.Conditions;
using QS.DomainModel.Tracking;

namespace QS.DomainModel.NotifyChange
{
	public class AppLevelChangeListener : ISingleUowEventsListnerFactory, IEntityChangeWatcher, IUowPostInsertEventListener, IUowPostUpdateEventListener, IUowPostDeleteEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly List<SubscriberWeakLink> SingleEventSubscribers = new List<SubscriberWeakLink>();
		private readonly List<SubscriberWeakLink> BatchEventSubscribers = new List<SubscriberWeakLink>();

		internal AppLevelChangeListener()
		{
		}

		#region Обработка уведомлений NHibernate

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			var change = new EntityChangeEvent(insertEvent);
			foreach (var subscriber in GetSingleSubscribers(change))
			{
				subscriber.Invoke(change);
			}
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent)
		{
			var change = new EntityChangeEvent(updateEvent);
			foreach (var subscriber in GetSingleSubscribers(change))
			{
				subscriber.Invoke(change);
			}
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			var change = new EntityChangeEvent(deleteEvent);
			foreach (var subscriber in GetSingleSubscribers(change))
			{
				subscriber.Invoke(change);
			}
		}

		private void RemoveSingleSubscriber(SubscriberWeakLink subscriber)
		{
			lock (SingleEventSubscribers)
			{
				SingleEventSubscribers.Remove(subscriber);
			}
		}

		private SubscriberWeakLink[] GetSingleSubscribers(EntityChangeEvent entityChange)
		{
			lock (SingleEventSubscribers)
			{
				SingleEventSubscribers.RemoveAll(s => !s.IsAlive);
				return SingleEventSubscribers.Where(x => x.IsSuitable(entityChange)).ToArray();
			}
		}

		#endregion

		#region Подписки

		/// <summary>
		/// Подписываемся на события изменения любых объектов.
		/// </summary>
		public void BatchSubscribeOnAll(BatchEntityChangeHandler subscriber)
		{
			lock (BatchEventSubscribers)
			{
				BatchEventSubscribers.Add(new SubscriberWeakLink(subscriber));
				logger.Debug($"Добавлена пакетная подписка на все изменения. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void BatchSubscribeOnEntity<TEntity>(BatchEntityChangeHandler subscriber)
		{
			lock (BatchEventSubscribers)
			{
				BatchEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity) }, subscriber));
				logger.Debug($"Добавлена пакетная подписка на изменение {typeof(TEntity)}. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2>(BatchEntityChangeHandler subscriber)
		{
			lock (BatchEventSubscribers)
			{
				BatchEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2) }, subscriber));
				logger.Debug($"Добавлена пакетная подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3>(BatchEntityChangeHandler subscriber)
		{
			lock (BatchEventSubscribers)
			{
				BatchEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2), typeof(TEntity3) }, subscriber));
				logger.Debug($"Добавлена пакетная подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}, {typeof(TEntity3)}. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void BatchSubscribeOnEntity<TEntity1, TEntity2, TEntity3, TEntity4>(BatchEntityChangeHandler subscriber)
		{
			lock (BatchEventSubscribers)
			{
				BatchEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2), typeof(TEntity3), typeof(TEntity4) }, subscriber));
				logger.Debug($"Добавлена пакетная подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}, {typeof(TEntity3)}, {typeof(TEntity4)}. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void BatchSubscribeOnEntity(BatchEntityChangeHandler subscriber, params Type[] entityClasses)
		{
			lock(BatchEventSubscribers) {
				BatchEventSubscribers.Add(new SubscriberWeakLink(entityClasses, subscriber));
				var list = String.Join(", ", entityClasses.Select(x => x.Name));
				logger.Debug($"Добавлена пакетная подписка на изменение {list}. Всего {BatchEventSubscribers.Count}");
			}
		}

		public void SingleSubscribeOnEntity<TEntity>(SingleEntityChangeEventMethod subscriber)
		{
			lock (SingleEventSubscribers)
			{
				SingleEventSubscribers.Add(new SubscriberWeakLink( typeof(TEntity) , subscriber));
				logger.Debug($"Добавлена Single-подписка на изменение {typeof(TEntity)}. Всего {SingleEventSubscribers.Count}");
			}
		}

		public SelectionConditions BatchSubscribe(BatchEntityChangeHandler subscriber)
		{
			lock(BatchEventSubscribers) {
				var condition = new SelectionConditions();
				BatchEventSubscribers.Add(new SubscriberWeakLink(condition, subscriber));
				logger.Debug($"Добавлена пакетная подписка с условиями. Всего {BatchEventSubscribers.Count}");
				return condition;
			}
		}

		#endregion

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
		{
			return new UowTracker(BatchEventSubscribers);
		}

		#region Отписка

		public void UnsubscribeAll(object owner)
		{
			var singleCount = SingleEventSubscribers.RemoveAll(s => s.Owner == owner);
			var manyCount = BatchEventSubscribers.RemoveAll(s => s.Owner == owner);
			logger.Debug($"{owner} отписался от уведомлениий Single={singleCount} Many={manyCount}");
		}

		public void Unsubscribe(SingleEntityChangeEventMethod subscriber)
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe(BatchEntityChangeHandler subscriber)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Внутриннее

		#endregion
	}
}
