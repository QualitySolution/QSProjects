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
			if(listener is IUowPreLoadEventListener)
				PreLoadListeners.Add(listener as IUowPreLoadEventListener);

			if(listener is IUowPostLoadEventListener)
				PostLoadListeners.Add(listener as IUowPostLoadEventListener);

			if(listener is IUowPostInsertEventListener)
				PostInsertListeners.Add(listener as IUowPostInsertEventListener);

			if(listener is IUowPostUpdateEventListener)
				PostUpdateListeners.Add(listener as IUowPostUpdateEventListener);

			if(listener is IUowPostDeleteEventListener)
				PostDeleteListeners.Add(listener as IUowPostDeleteEventListener);

			if (listener is IUowPostCommitEventListener)
				PostCommitListeners.Add(listener as IUowPostCommitEventListener);
		}

		public void UnregisterListener(object listener)
		{
			if(listener is IUowPreLoadEventListener)
				PreLoadListeners.Remove(listener as IUowPreLoadEventListener);

			if(listener is IUowPostLoadEventListener)
				PostLoadListeners.Remove(listener as IUowPostLoadEventListener);

			if(listener is IUowPostInsertEventListener)
				PostInsertListeners.Remove(listener as IUowPostInsertEventListener);

			if(listener is IUowPostUpdateEventListener)
				PostUpdateListeners.Remove(listener as IUowPostUpdateEventListener);

			if(listener is IUowPostDeleteEventListener)
				PostDeleteListeners.Remove(listener as IUowPostDeleteEventListener);

			if (listener is IUowPostCommitEventListener)
				PostCommitListeners.Remove(listener as IUowPostCommitEventListener);
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
