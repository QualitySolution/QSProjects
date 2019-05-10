using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using QS.DomainModel.Tracking;

namespace QS.DomainModel.NotifyChange
{
	public class AppLevelChangeListener : ISingleUowEventsListnerFactory, IEntityChangeWatcher, IUowPostInsertEventListener, IUowPostUpdateEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly List<SubscriberWeakLink> SingleEventSubscribers = new List<SubscriberWeakLink>();
		private readonly List<SubscriberWeakLink> ManyEventSubscribers = new List<SubscriberWeakLink>();

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
		public void WatchManyAll(ManyEntityChangeEventMethod subscriber)
		{
			lock (ManyEventSubscribers)
			{
				ManyEventSubscribers.Add(new SubscriberWeakLink(subscriber));
				logger.Debug($"Добавлена Many-подписка на все изменения. Всего {ManyEventSubscribers.Count}");
			}
		}

		public void WatchMany<TEntity>(ManyEntityChangeEventMethod subscriber)
		{
			lock (ManyEventSubscribers)
			{
				ManyEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity) }, subscriber));
				logger.Debug($"Добавлена Many-подписка на изменение {typeof(TEntity)}. Всего {ManyEventSubscribers.Count}");
			}
		}

		public void WatchMany<TEntity1, TEntity2>(ManyEntityChangeEventMethod subscriber)
		{
			lock (ManyEventSubscribers)
			{
				ManyEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2) }, subscriber));
				logger.Debug($"Добавлена Many-подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}. Всего {ManyEventSubscribers.Count}");
			}
		}

		public void WatchMany<TEntity1, TEntity2, TEntity3>(ManyEntityChangeEventMethod subscriber)
		{
			lock (ManyEventSubscribers)
			{
				ManyEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2), typeof(TEntity3) }, subscriber));
				logger.Debug($"Добавлена Many-подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}, {typeof(TEntity3)}. Всего {ManyEventSubscribers.Count}");
			}
		}

		public void WatchMany<TEntity1, TEntity2, TEntity3, TEntity4>(ManyEntityChangeEventMethod subscriber)
		{
			lock (ManyEventSubscribers)
			{
				ManyEventSubscribers.Add(new SubscriberWeakLink(new[] { typeof(TEntity1), typeof(TEntity2), typeof(TEntity3), typeof(TEntity4) }, subscriber));
				logger.Debug($"Добавлена Many-подписка на изменение {typeof(TEntity1)}, {typeof(TEntity2)}, {typeof(TEntity3)}, {typeof(TEntity4)}. Всего {ManyEventSubscribers.Count}");
			}
		}

		public void WatchOne<TEntity>(SingleEntityChangeEventMethod subscriber)
		{
			lock (SingleEventSubscribers)
			{
				SingleEventSubscribers.Add(new SubscriberWeakLink( typeof(TEntity) , subscriber));
				logger.Debug($"Добавлена Single-подписка на изменение {typeof(TEntity)}. Всего {SingleEventSubscribers.Count}");
			}
		}
		#endregion

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
		{
			return new UowTracker(ManyEventSubscribers);
		}

		#region Отписка

		public void UnsubscribeAll(object owner)
		{
			var singleCount = SingleEventSubscribers.RemoveAll(s => s.Owner == owner);
			var manyCount = ManyEventSubscribers.RemoveAll(s => s.Owner == owner);
			logger.Debug($"{owner} отписался от уведомлениий Single={singleCount} Many={manyCount}");
		}

		public void Unsubscribe(SingleEntityChangeEventMethod subscriber)
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe(ManyEntityChangeEventMethod subscriber)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Внутриннее

		#endregion
	}
}
