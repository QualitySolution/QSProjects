using System;
using System.Collections.Generic;
using NHibernate.Event;
using QSProjectsLib;

namespace QS.DomainModel
{
	public class NhEventListener: IPostLoadEventListener, IPreLoadEventListener
	{
#region Статическое
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		internal static readonly List<IUnitOfWorkEventHandler> RegisteredUoWs = new List<IUnitOfWorkEventHandler>() ;

		internal static void RegisterUow(IUnitOfWorkEventHandler uow)
		{
			RegisteredUoWs.Add(uow);
			logger.Debug("Зарегистрирован новый UnitOfWork. {0}", ActiveUowCountText());
		}

		internal static void UnregisterUow(IUnitOfWorkEventHandler uow)
		{
			RegisteredUoWs.Remove(uow);
			logger.Debug("UnitOfWork завершил работу. {0}", ActiveUowCountText());
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
			bool notFound = true;
			foreach(var uow in RegisteredUoWs)
			{
				if(uow.Session == @event.Session)
				{
					uow.OnPostLoad(@event);
					notFound = false;
				}
			}

			if(notFound)
				logger.Warn("Пришло событие PostLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}

		public void OnPreLoad(PreLoadEvent @event)
		{
			bool notFound = true;
			foreach(var uow in RegisteredUoWs) {
				if(uow.Session == @event.Session) {
					uow.OnPreLoad(@event);
					notFound = false;
				}
			}

			if(notFound)
				logger.Warn("Пришло событие PreLoadEvent но соответствующий сессии UnitOfWork не найден.");
		}
	}
}
