using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using QS.DomainModel.Tracking;
using QSProjectsLib;

namespace QS.DomainModel
{
	public class NhEventListener: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener
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
			return RusNumber.FormatCase(RegisteredUoWs.Count, "Сейчас {0} активный UnitOfWork.", "Сейчас {0} активных UnitOfWork.", "Сейчас {0} активных UnitOfWork.");
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
				uow.OnPostDelete(@event);
			else
				logger.Warn("Пришло событие PostDeleteEvent но соответствующий сессии UnitOfWork не найден.");
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
