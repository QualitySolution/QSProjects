using System;
using System.Collections.Generic;
using NHibernate.Event;

namespace QS.DomainModel.Tracking
{
	public class SingleUowEventsTracker
	{
		#region Static

		private static readonly HashSet<ISingleUowEventsListnerFactory> SingleUowListnerFactories = new HashSet<ISingleUowEventsListnerFactory>();

		public static void RegisterSingleUowListnerFactory(ISingleUowEventsListnerFactory factory)
		{
			lock (SingleUowListnerFactories)
			{
				SingleUowListnerFactories.Add(factory);
			}
		}

		public static void UnregisterSingleUowListnerFactory(ISingleUowEventsListnerFactory factory)
		{
			lock (SingleUowListnerFactories)
			{
				SingleUowListnerFactories.Remove(factory);
			}
		}

		static SingleUowEventsTracker()
		{
			UowWatcher.UowRegistered += UowWatcher_UowRegistered;
		}

		static void UowWatcher_UowRegistered(object sender, UowRegistereEventArgs e)
		{
			foreach (var factory in SingleUowListnerFactories)
			{
				var listner = factory.CreateListnerForNewUow(e.UoW);
				e.UoW.EventsTracker.RegisterListener(listner);
			}
		}

		#endregion


		private readonly HashSet<IUowPreLoadEventListener> PreLoadListeners = new HashSet<IUowPreLoadEventListener>();
		private readonly HashSet<IUowPostLoadEventListener> PostLoadListeners = new HashSet<IUowPostLoadEventListener>();
		private readonly HashSet<IUowPostInsertEventListener> PostInsertListeners = new HashSet<IUowPostInsertEventListener>();
		private readonly HashSet<IUowPostUpdateEventListener> PostUpdateListeners = new HashSet<IUowPostUpdateEventListener>();
		private readonly HashSet<IUowPostDeleteEventListener> PostDeleteListeners = new HashSet<IUowPostDeleteEventListener>();
		private readonly HashSet<IUowPostCommitEventListener> PostCommitListeners = new HashSet<IUowPostCommitEventListener>();

		public SingleUowEventsTracker()
		{
		}

		public void RegisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener preLoadEventListener)
				PreLoadListeners.Add(preLoadEventListener);

			if(listener is IUowPostLoadEventListener postLoadEventListener)
				PostLoadListeners.Add(postLoadEventListener);

			if(listener is IUowPostInsertEventListener postInsertEventListener)
				PostInsertListeners.Add(postInsertEventListener);

			if(listener is IUowPostUpdateEventListener postUpdateEventListener)
				PostUpdateListeners.Add(postUpdateEventListener);

			if(listener is IUowPostDeleteEventListener postDeleteEventListener)
				PostDeleteListeners.Add(postDeleteEventListener);

			if(listener is IUowPostCommitEventListener postCommitEventListener)
				PostCommitListeners.Add(postCommitEventListener);
		}

		public void UnregisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener preLoadEventListener)
				PreLoadListeners.Remove(preLoadEventListener);

			if(listener is IUowPostLoadEventListener postLoadEventListener)
				PostLoadListeners.Remove(postLoadEventListener);

			if(listener is IUowPostInsertEventListener postInsertEventListener)
				PostInsertListeners.Remove(postInsertEventListener);

			if(listener is IUowPostUpdateEventListener postUpdateEventListener)
				PostUpdateListeners.Remove(postUpdateEventListener);

			if(listener is IUowPostDeleteEventListener postDeleteEventListener)
				PostDeleteListeners.Remove(postDeleteEventListener);

			if(listener is IUowPostCommitEventListener postCommitEventListener)
				PostCommitListeners.Remove(postCommitEventListener);
		}

		public void OnPreLoad(IUnitOfWorkTracked uow, PreLoadEvent loadEvent)
		{
			foreach(var listner in PreLoadListeners)
				listner.OnPreLoad(uow, loadEvent);
		}

		public void OnPostLoad(IUnitOfWorkTracked uow, PostLoadEvent loadEvent)
		{
			foreach(var listner in PostLoadListeners)
				listner.OnPostLoad(uow, loadEvent);
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			foreach(var listner in PostInsertListeners)
				listner.OnPostInsert(uow, insertEvent);
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent) 
		{
			foreach(var listner in PostUpdateListeners)
				listner.OnPostUpdate(uow, updateEvent);
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			foreach(var listner in PostDeleteListeners)
				listner.OnPostDelete(uow, deleteEvent);
		}

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
			foreach (var listner in PostCommitListeners)
				listner.OnPostCommit(uow);
		}
	}
}
