using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using QS.DomainModel.Tracking;

namespace QS.DomainModel.NotifyChange
{
	public class UowTracker : ISingleUowEventListener, IUowPostInsertEventListener, IUowPostUpdateEventListener, IUowPostCommitEventListener, IUowPostDeleteEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly List<EntityChangeEvent> entityChanges = new List<EntityChangeEvent>();
		private readonly List<SubscriberWeakLink> eventSubscribers;

		public UowTracker(List<SubscriberWeakLink> subscriberWeakLinks)
		{
			eventSubscribers = subscriberWeakLinks;
		}

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
			SubscriberWeakLink[] subscribers;
			lock (eventSubscribers) {
				int counts = eventSubscribers.Count;
				eventSubscribers.RemoveAll(s => !s.IsAlive);
				subscribers = eventSubscribers.ToArray();
				if(eventSubscribers.Count != counts)
					logger.Debug($"Очищено {counts - eventSubscribers.Count} протухших подписчиков. Осталось {eventSubscribers.Count} подписчиков.");
			}
			foreach (var subscriber in subscribers)
			{
				var changes = entityChanges.Where(subscriber.IsSuitable).ToArray();
				if (changes.Length > 0)
				{
					logger.Debug($"Вызываем подписчика [{subscriber}]");
					subscriber.Invoke(changes);
				}
			}
			entityChanges.Clear();
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			var change = new EntityChangeEvent(insertEvent);
			entityChanges.Add(change);
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent)
		{
			var change = new EntityChangeEvent(updateEvent);
			entityChanges.Add(change);
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			var change = new EntityChangeEvent(deleteEvent);
			entityChanges.Add(change);
		}
	}
}
