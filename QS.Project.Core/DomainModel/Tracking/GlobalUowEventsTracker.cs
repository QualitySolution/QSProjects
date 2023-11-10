using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
 using NHibernate;
 using NHibernate.Event;
using QS.Dialog;

namespace QS.DomainModel.Tracking
{
	public class GlobalUowEventsTracker: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener, IPostUpdateEventListener, IPostInsertEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static readonly HashSet<IUowPreLoadEventListener> PreLoadListeners = new HashSet<IUowPreLoadEventListener>();
		private static readonly HashSet<IUowPostLoadEventListener> PostLoadListeners = new HashSet<IUowPostLoadEventListener>();
		private static readonly HashSet<IUowPostInsertEventListener> PostInsertListeners = new HashSet<IUowPostInsertEventListener>();
		private static readonly HashSet<IUowPostUpdateEventListener> PostUpdateListeners = new HashSet<IUowPostUpdateEventListener>();
		private static readonly HashSet<IUowPostDeleteEventListener> PostDeleteListeners = new HashSet<IUowPostDeleteEventListener>();
		private static readonly HashSet<IUowPostCommitEventListener> PostCommitListeners = new HashSet<IUowPostCommitEventListener>();
		private readonly IMainThreadDispatcher threadDispatcher;


		public GlobalUowEventsTracker(IMainThreadDispatcher threadDispatcher) {
			this.threadDispatcher = threadDispatcher ?? throw new ArgumentNullException(nameof(threadDispatcher));
		}

		#region Register



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
					SafeInvoke(() => listner.OnPreLoad(uow, @event));
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
					SafeInvoke(() => listner.OnPostLoad(uow, @event));
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
					SafeInvoke(() => listner.OnPostInsert(uow, @event));
				}
			}

			uow.EventsTracker.OnPostInsert(uow, @event);
		}

		private object lastPostUpdateEntity;
		private ITransaction lastPostUpdateTransaction;

		public void OnPostUpdate(PostUpdateEvent @event)
		{ 
			//Из-за бага\фичи в Nh, приходят по 2 одинаковых события.
			var currentTransaction = @event.Session.GetCurrentTransaction();
			if(lastPostUpdateEntity == @event.Entity && lastPostUpdateTransaction == currentTransaction)
				return;
			lastPostUpdateEntity = @event.Entity;
			lastPostUpdateTransaction = currentTransaction;

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
					SafeInvoke(() => listner.OnPostUpdate(uow, @event));
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
					SafeInvoke(() => listner.OnPostDelete(uow, @event));
				}
			}

			uow.EventsTracker.OnPostDelete(uow, @event);
		}

		/// <summary>
		/// Вызывается только из Uow
		/// </summary>
		public static void OnPostCommit(IUnitOfWorkTracked uow)
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

		private void SafeInvoke(Action action) {
			if(threadDispatcher.MainThread == Thread.CurrentThread) {
				action.Invoke();
			}
			else {
				threadDispatcher.RunInMainTread(action);
			}
		}

		private IUnitOfWorkTracked GetUnitOfWork(IEventSource session)
		{
			return UowWatcher.GetUnitOfWork(session) as IUnitOfWorkTracked;
		}

		
		public async Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
		{
			await Task.Run(() => OnPostInsert(@event));
		}

		public async Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
		{
			await Task.Run(() => OnPostUpdate(@event));
		}

		public async Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
		{
			await Task.Run(() => OnPostDelete(@event));
		}

		public async Task OnPreLoadAsync(PreLoadEvent @event, CancellationToken cancellationToken)
		{
			await Task.Run(() => OnPreLoad(@event));
		}
	}
}
