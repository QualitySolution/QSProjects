using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using QSProjectsLib;

namespace QS.DomainModel
{
	public class NhEventListener: IPostLoadEventListener, IPreLoadEventListener, IPostDeleteEventListener
	{
#region Статическое
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		internal static readonly List<IUnitOfWorkEventHandler> RegisteredUoWs = new List<IUnitOfWorkEventHandler>() ;

		internal static void RegisterUow(IUnitOfWorkEventHandler uow)
		{
			lock(RegisteredUoWs)
			{
				RegisteredUoWs.Add(uow);
				logger.Debug("Зарегистрирован новый UnitOfWork. {0}", ActiveUowCountText());
			}
		}

		internal static void UnregisterUow(IUnitOfWorkEventHandler uow)
		{
			lock(RegisteredUoWs)
			{
				RegisteredUoWs.Remove(uow);
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
			IUnitOfWorkEventHandler uow;
			lock (RegisteredUoWs)
			{
				uow = RegisteredUoWs.FirstOrDefault(u => u.Session == @event.Session);
			}

			if (uow != null)
				uow.OnPostLoad(@event);
			else
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPreLoad(PreLoadEvent @event)
		{
			IUnitOfWorkEventHandler uow;
			lock (RegisteredUoWs)
			{
				uow = RegisteredUoWs.FirstOrDefault(u => u.Session == @event.Session);
			}

			if (uow != null)
				uow.OnPreLoad(@event);
			else
				logger.Warn("Пришло событие PreLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPostDelete(PostDeleteEvent @event)
		{
			IUnitOfWorkEventHandler uow;
			lock(RegisteredUoWs)
			{
				uow = RegisteredUoWs.FirstOrDefault(u => u.Session == @event.Session);
			}

			if(uow != null)
				uow.OnPostDelete(@event);
			else
				logger.Warn("Пришло событие PostDeleteEvent но соответствующий сессии UnitOfWork не найден.");
		}
	}
}
