using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;

namespace QS.DomainModel.Tracking
{
	public class NhEventListener: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener, IPostUpdateEventListener, IPostInsertEventListener
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region PreLoad

		private static readonly HashSet<IUowPreLoadEventListener> PreLoadListeners = new HashSet<IUowPreLoadEventListener>();

		public static bool RegisterPreLoadListener(IUowPreLoadEventListener listener)
		{
			lock(PreLoadListeners)
			{
				return PreLoadListeners.Add(listener);
			}
		}

		public static bool UnregisterPreLoadListener(IUowPreLoadEventListener listener)
		{
			lock (PreLoadListeners)
			{
				return PreLoadListeners.Remove(listener);
			}
		}

		public void OnPreLoad(PreLoadEvent @event)
		{
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
		}

		#endregion

		public NhEventListener()
		{
		}

		public void OnPostLoad(PostLoadEvent @event)
		{
            IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);

			if (uow != null)
				uow.OnPostLoad(@event);
			else
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}


		public void OnPostDelete(PostDeleteEvent @event)
		{
			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);

			if(uow != null)
			{
				uow.HibernateTracker?.OnPostDelete(@event);
				uow.OnPostDelete(@event);
			}
			else
				logger.Warn("Пришло событие PostDeleteEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPostUpdate(PostUpdateEvent @event)
		{
			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);

			if(uow != null)
			{
				uow.HibernateTracker?.OnPostUpdate(@event);
			}
			else
				logger.Warn("Пришло событие OnPostUpdate но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPostInsert(PostInsertEvent @event)
		{
			IUnitOfWorkTracked uow = GetUnitOfWork(@event.Session);

			if(uow != null) {
				uow.HibernateTracker?.OnPostInsert(@event);
			} else
				logger.Warn("Пришло событие OnPostUpdate но соответствующий сессии UnitOfWork не найден.");
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
