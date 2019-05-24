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
		private readonly List<SubscriberWeakLink> ManyEventSubscribers;

		public UowTracker(List<SubscriberWeakLink> subscriberWeakLinks)
		{
			ManyEventSubscribers = subscriberWeakLinks;
		}

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
			SubscriberWeakLink[] subscribers;
			lock (ManyEventSubscribers) {
				int counts = ManyEventSubscribers.Count;
				ManyEventSubscribers.RemoveAll(s => !s.IsAlive);
				subscribers = ManyEventSubscribers.ToArray();
				if(ManyEventSubscribers.Count != counts)
					logger.Debug($"Очищено {counts - ManyEventSubscribers.Count} протухших подписчиков. Осталось {ManyEventSubscribers.Count} подписчиков.");
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
