using NHibernate.Event;
using System.Collections.Generic;

namespace QS.DomainModel.Tracking {
	public class SingleUowEventsTracker
	{
		#region Static

		private static readonly HashSet<ISingleUowEventsListnerFactory> singleUowListnerFactories = new HashSet<ISingleUowEventsListnerFactory>();

		public static void RegisterSingleUowListnerFactory(ISingleUowEventsListnerFactory factory)
		{
			lock (singleUowListnerFactories)
			{
				singleUowListnerFactories.Add(factory);
			}
		}

		public static void UnregisterSingleUowListnerFactory(ISingleUowEventsListnerFactory factory)
		{
			lock (singleUowListnerFactories)
			{
				singleUowListnerFactories.Remove(factory);
			}
		}

		static SingleUowEventsTracker()
		{
			UowWatcher.UowRegistered += UowWatcher_UowRegistered;
		}

		static void UowWatcher_UowRegistered(object sender, UowRegistereEventArgs e)
		{
			foreach (var factory in singleUowListnerFactories)
			{
				var listner = factory.CreateListnerForNewUow(e.UoW);
				e.UoW.EventsTracker?.RegisterListener(listner);
			}
		}

		#endregion

		private readonly HashSet<IUowPreLoadEventListener> preLoadListeners = new HashSet<IUowPreLoadEventListener>();
		private readonly HashSet<IUowPostLoadEventListener> postLoadListeners = new HashSet<IUowPostLoadEventListener>();
		private readonly HashSet<IUowPostInsertEventListener> postInsertListeners = new HashSet<IUowPostInsertEventListener>();
		private readonly HashSet<IUowPostUpdateEventListener> postUpdateListeners = new HashSet<IUowPostUpdateEventListener>();
		private readonly HashSet<IUowPostDeleteEventListener> postDeleteListeners = new HashSet<IUowPostDeleteEventListener>();
		private readonly HashSet<IUowPostCommitEventListener> postCommitListeners = new HashSet<IUowPostCommitEventListener>();

		public void RegisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener preLoadEventListener)
				preLoadListeners.Add(preLoadEventListener);

			if(listener is IUowPostLoadEventListener postLoadEventListener)
				postLoadListeners.Add(postLoadEventListener);

			if(listener is IUowPostInsertEventListener postInsertEventListener)
				postInsertListeners.Add(postInsertEventListener);

			if(listener is IUowPostUpdateEventListener postUpdateEventListener)
				postUpdateListeners.Add(postUpdateEventListener);

			if(listener is IUowPostDeleteEventListener postDeleteEventListener)
				postDeleteListeners.Add(postDeleteEventListener);

			if(listener is IUowPostCommitEventListener postCommitEventListener)
				postCommitListeners.Add(postCommitEventListener);
		}

		public void UnregisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener preLoadEventListener)
				preLoadListeners.Remove(preLoadEventListener);

			if(listener is IUowPostLoadEventListener postLoadEventListener)
				postLoadListeners.Remove(postLoadEventListener);

			if(listener is IUowPostInsertEventListener postInsertEventListener)
				postInsertListeners.Remove(postInsertEventListener);

			if(listener is IUowPostUpdateEventListener postUpdateEventListener)
				postUpdateListeners.Remove(postUpdateEventListener);

			if(listener is IUowPostDeleteEventListener postDeleteEventListener)
				postDeleteListeners.Remove(postDeleteEventListener);

			if(listener is IUowPostCommitEventListener postCommitEventListener)
				postCommitListeners.Remove(postCommitEventListener);
		}

		public void OnPreLoad(IUnitOfWorkTracked uow, PreLoadEvent loadEvent)
		{
			foreach(var listner in preLoadListeners) {
				listner.OnPreLoad(uow, loadEvent);
			}
		}

		public void OnPostLoad(IUnitOfWorkTracked uow, PostLoadEvent loadEvent)
		{
			foreach(var listner in postLoadListeners) {
				listner.OnPostLoad(uow, loadEvent);
			}
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			foreach(var listner in postInsertListeners) {
				listner.OnPostInsert(uow, insertEvent);
			}
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent) 
		{
			foreach(var listner in postUpdateListeners) {
				listner.OnPostUpdate(uow, updateEvent);
			}
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			foreach(var listner in postDeleteListeners) {
				listner.OnPostDelete(uow, deleteEvent);
			}
		}

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
			foreach (var listner in postCommitListeners) {
				listner.OnPostCommit(uow);
			}
		}
	}
}
