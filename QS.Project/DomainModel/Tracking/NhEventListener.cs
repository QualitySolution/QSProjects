using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;
using QS.DomainModel.UoW;
using QS.Utilities;

namespace QS.DomainModel.Tracking
{
	public class NhEventListener: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener, IPostUpdateEventListener, IPostInsertEventListener
	{
#region Статическое
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		internal static readonly Dictionary<int, UowLink> RegisteredUoWs = new Dictionary<int, UowLink>() ;

		internal static void RegisterUow(IUnitOfWorkEventHandler uow)
		{
            RemoveLost();
            lock (RegisteredUoWs)
			{
				var uowLink = new UowLink(uow);
				RegisteredUoWs.Add(uow.Session.GetHashCode(), uowLink);

				logger.Debug($"Зарегистрирован новый UnitOfWork. {ActiveUowCountText()}. Создан в {uowLink.Title.CallerMemberName} ({uowLink.Title.CallerFilePath}:{uowLink.Title.CallerLineNumber})");
			}
		}

		internal static void UnregisterUow(IUnitOfWorkEventHandler uow)
		{
            RemoveLost();
            lock (RegisteredUoWs)
			{
                RegisteredUoWs.Remove(uow.Session.GetHashCode());
				logger.Debug("UnitOfWork завершил работу. {0}", ActiveUowCountText());
			}
		}

		private static string ActiveUowCountText()
		{
			return NumberToTextRus.FormatCase(RegisteredUoWs.Count, "Сейчас {0} активный UnitOfWork.", "Сейчас {0} активных UnitOfWork.", "Сейчас {0} активных UnitOfWork.");
		}
#endregion

		public NhEventListener()
		{
		}

		public void OnPostLoad(PostLoadEvent @event)
		{
            IUnitOfWorkEventHandler uow = GetUnitOfWork(@event.Session);

			if (uow != null)
				uow.OnPostLoad(@event);
			else
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPreLoad(PreLoadEvent @event)
		{
			IUnitOfWorkEventHandler uow = GetUnitOfWork(@event.Session);

			if (uow != null)
				uow.OnPreLoad(@event);
			else
				logger.Warn("Пришло событие PreLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPostDelete(PostDeleteEvent @event)
		{
			IUnitOfWorkEventHandler uow = GetUnitOfWork(@event.Session);

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
			IUnitOfWorkEventHandler uow = GetUnitOfWork(@event.Session);

			if(uow != null)
			{
				uow.HibernateTracker?.OnPostUpdate(@event);
			}
			else
				logger.Warn("Пришло событие OnPostUpdate но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPostInsert(PostInsertEvent @event)
		{
			IUnitOfWorkEventHandler uow = GetUnitOfWork(@event.Session);

			if(uow != null) {
				uow.HibernateTracker?.OnPostInsert(@event);
			} else
				logger.Warn("Пришло событие OnPostUpdate но соответствующий сессии UnitOfWork не найден.");
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

        private IUnitOfWorkEventHandler GetUnitOfWork(NHibernate.ISession session)
        {
            lock (RegisteredUoWs)
            {
                int hashCode = session.GetHashCode();
                if(RegisteredUoWs.ContainsKey(hashCode))
                {
                    return RegisteredUoWs[hashCode].UnitOfWork;
                }
            }
            return null;
        }

        static DateTime nextCheck;

		private static void RemoveLost()
        {
            lock (RegisteredUoWs)
            {
                if (DateTime.Now < nextCheck)
                    return;

                foreach (var pair in RegisteredUoWs.ToList())
                {
                    if (pair.Value.UnitOfWork == null)
                    {
                        logger.Warn($"UnitOfWork созданный в {pair.Value.Title.CallerMemberName}({pair.Value.Title.CallerFilePath}:{pair.Value.Title.CallerLineNumber}) не был закрыт корректно и удален сборщиком мусора так как на него потеряны все ссылки.");
                        RegisteredUoWs.Remove(pair.Key);
                    }
                }

                nextCheck = DateTime.Now.AddSeconds(5);
            }
        }
	}

	internal class UowLink
    {
        WeakReference uow;
        
        public UnitOfWorkTitle Title { get; private set; }

        public int SessionHashCode { get; private set; }

        public UowLink(IUnitOfWorkEventHandler uow)
        {
            SessionHashCode = uow.Session.GetHashCode();
            this.uow = new WeakReference(uow);
            this.Title = uow.ActionTitle;
        }

        public IUnitOfWorkEventHandler UnitOfWork => uow.Target as IUnitOfWorkEventHandler;
    }
}
