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
			if(listener is IUowPreLoadEventListener)
				preLoadListeners.Add(listener as IUowPreLoadEventListener);

			if(listener is IUowPostLoadEventListener)
				postLoadListeners.Add(listener as IUowPostLoadEventListener);

			if(listener is IUowPostInsertEventListener)
				postInsertListeners.Add(listener as IUowPostInsertEventListener);

			if(listener is IUowPostUpdateEventListener)
				postUpdateListeners.Add(listener as IUowPostUpdateEventListener);

			if(listener is IUowPostDeleteEventListener)
				postDeleteListeners.Add(listener as IUowPostDeleteEventListener);

			if (listener is IUowPostCommitEventListener)
				postCommitListeners.Add(listener as IUowPostCommitEventListener);
		}

		public void UnregisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener)
				preLoadListeners.Remove(listener as IUowPreLoadEventListener);

			if(listener is IUowPostLoadEventListener)
				postLoadListeners.Remove(listener as IUowPostLoadEventListener);

			if(listener is IUowPostInsertEventListener)
				postInsertListeners.Remove(listener as IUowPostInsertEventListener);

			if(listener is IUowPostUpdateEventListener)
				postUpdateListeners.Remove(listener as IUowPostUpdateEventListener);

			if(listener is IUowPostDeleteEventListener)
				postDeleteListeners.Remove(listener as IUowPostDeleteEventListener);

			if (listener is IUowPostCommitEventListener)
				postCommitListeners.Remove(listener as IUowPostCommitEventListener);
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
