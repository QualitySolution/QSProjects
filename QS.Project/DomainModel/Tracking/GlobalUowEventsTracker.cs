using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;

namespace QS.DomainModel.Tracking
{
	public class GlobalUowEventsTracker: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener, IPostUpdateEventListener, IPostInsertEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Register

		private static readonly HashSet<IUowPreLoadEventListener> PreLoadListeners = new HashSet<IUowPreLoadEventListener>();
		private static readonly HashSet<IUowPostLoadEventListener> PostLoadListeners = new HashSet<IUowPostLoadEventListener>();
		private static readonly HashSet<IUowPostInsertEventListener> PostInsertListeners = new HashSet<IUowPostInsertEventListener>();
		private static readonly HashSet<IUowPostUpdateEventListener> PostUpdateListeners = new HashSet<IUowPostUpdateEventListener>();
		private static readonly HashSet<IUowPostDeleteEventListener> PostDeleteListeners = new HashSet<IUowPostDeleteEventListener>();
		private static readonly HashSet<IUowPostCommitEventListener> PostCommitListeners = new HashSet<IUowPostCommitEventListener>();

		public static void RegisterGlobalListener(object listener)
		{
			if (listener is IUowPreLoadEventListener)
				lock (PreLoadListeners)
				{
					PreLoadListeners.Add(listener as IUowPreLoadEventListener);
				}

			if (listener is IUowPostLoadEventListener)
				lock(PostLoadListeners)
				{
					PostLoadListeners.Add(listener as IUowPostLoadEventListener);
				}

			if (listener is IUowPostInsertEventListener)
				lock(PostInsertListeners)
				{
					PostInsertListeners.Add(listener as IUowPostInsertEventListener);
				}

			if (listener is IUowPostUpdateEventListener)
				lock(PostUpdateListeners)
				{
					PostUpdateListeners.Add(listener as IUowPostUpdateEventListener);
				}

			if (listener is IUowPostDeleteEventListener)
				lock(PostDeleteListeners)
				{
					PostDeleteListeners.Add(listener as IUowPostDeleteEventListener);
				}

			if (listener is IUowPostCommitEventListener)
				lock(PostCommitListeners) {
					PostCommitListeners.Add(listener as IUowPostCommitEventListener);
				}
		}

		public static void UnregisterGlobalListener(object listener)
		{
			if (listener is IUowPreLoadEventListener)
				lock (PreLoadListeners)
				{
					PreLoadListeners.Remove(listener as IUowPreLoadEventListener);
				}

			if (listener is IUowPostLoadEventListener)
				lock (PostLoadListeners)
				{
					PostLoadListeners.Remove(listener as IUowPostLoadEventListener);
				}

			if (listener is IUowPostInsertEventListener)
				lock (PostInsertListeners)
				{
					PostInsertListeners.Remove(listener as IUowPostInsertEventListener);
				}

			if (listener is IUowPostUpdateEventListener)
				lock (PostUpdateListeners)
				{
					PostUpdateListeners.Remove(listener as IUowPostUpdateEventListener);
				}

			if (listener is IUowPostDeleteEventListener)
				lock (PostDeleteListeners)
				{
					PostDeleteListeners.Remove(listener as IUowPostDeleteEventListener);
				}

			if(listener is IUowPostCommitEventListener)
				lock(PostCommitListeners) {
					PostCommitListeners.Remove(listener as IUowPostCommitEventListener);
				}
		}

		#endregion

		#region Events

		private object lastPreLoadEntity;

		public void OnPreLoad(PreLoadEvent @event)
		{
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			if(lastPreLoadEntity == @event.Entity)
				return;
			lastPreLoadEntity = @event.Entity;

			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);
			if (uow == null)
			{
				logger.Warn("Пришло событие PreLoadEvent но соответствующий сессии UnitOfWork не найден.");
				return;
			}

			lock (PreLoadListeners)
			{
				foreach (var listner in PreLoadListeners)
				{
					listner.OnPreLoad(uow, @event);
				}
			}

			uow.EventsTracker.OnPreLoad(uow, @event);
		}

		private object lastPostLoadEntity;

		public void OnPostLoad(PostLoadEvent @event)
		{
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			if(lastPostLoadEntity == @event.Entity)
				return;
			lastPostLoadEntity = @event.Entity;

			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);
			if (uow == null)
			{
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
				return;
			}

			lock (PostLoadListeners)
			{
				foreach (var listner in PostLoadListeners)
				{
					listner.OnPostLoad(uow, @event);
				}
			}

			uow.EventsTracker.OnPostLoad(uow, @event);
		}

		private object lastPostInsertEntity;

		public void OnPostInsert(PostInsertEvent @event)
		{
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			if(lastPostInsertEntity == @event.Entity)
				return;
			lastPostInsertEntity = @event.Entity;

			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);
			if (uow == null)
			{
				logger.Warn("Пришло событие PostInsertEvent но соответствующий сессии UnitOfWork не найден.");
				return;
			}

			lock (PostInsertListeners)
			{
				foreach (var listner in PostInsertListeners)
				{
					listner.OnPostInsert(uow, @event);
				}
			}

			uow.EventsTracker.OnPostInsert(uow, @event);
		}

		private object lastPostUpdateEntity;

		public void OnPostUpdate(PostUpdateEvent @event)
		{
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			if(lastPostUpdateEntity == @event.Entity)
				return;
			lastPostUpdateEntity = @event.Entity;

			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);
			if (uow == null)
			{
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
				return;
			}

			lock (PostUpdateListeners)
			{
				foreach (var listner in PostUpdateListeners)
				{
					listner.OnPostUpdate(uow, @event);
				}
			}

			uow.EventsTracker.OnPostUpdate(uow, @event);
		}

		private object lastPostDeleteEntity;

		public void OnPostDelete(PostDeleteEvent @event)
		{
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			if(lastPostDeleteEntity == @event.Entity)
				return;
			lastPostDeleteEntity = @event.Entity;

			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);
			if (uow == null)
			{
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
				return;
			}

			lock (PostDeleteListeners)
			{
				foreach (var listner in PostDeleteListeners)
				{
					listner.OnPostDelete(uow, @event);
				}
			}

			uow.EventsTracker.OnPostDelete(uow, @event);
		}

		/// <summary>
		/// Вызывается только из Uow
		/// </summary>
		internal static void OnPostCommit(IUnitOfWorkTracked uow)
		{
			lock (PostCommitListeners)
			{
				foreach (var listner in PostCommitListeners)
				{
					listner.OnPostCommit(uow);
				}
			}

			uow.EventsTracker.OnPostCommit(uow);
		}

		#endregion

		public GlobalUowEventsTracker()
		{
		}

		private IUnitOfWorkTracked GetUnitOfWork(IEventSource session)
		{
			return UowWatcher.GetUnitOfWork(session) as IUnitOfWorkTracked;
		}

		public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task OnPreLoadAsync(PreLoadEvent @event, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
