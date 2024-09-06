using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Engine;
using QS.DomainModel.UoW;
using QS.Utilities;

namespace QS.DomainModel.Tracking
{
	public static class UowWatcher
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		internal static readonly Dictionary<Guid, UowLink> RegisteredUoWs = new Dictionary<Guid, UowLink>();

		public static event EventHandler<UowRegistereEventArgs> UowRegistered;

		internal static void RegisterUow(IUnitOfWorkTracked uow)
		{
			RemoveLost();
			lock (RegisteredUoWs)
			{
				var uowLink = new UowLink(uow);
				var sessionId = (uow.Session as ISessionImplementor).SessionId;
				RegisteredUoWs.Add(sessionId, uowLink);
				logger.Debug($"Зарегистрирован новый UnitOfWork. {ActiveUowCountText()}." +
					$" Создан в {uowLink.Title.UserActionTitle} {uowLink.Title.CallerMemberName} ({uowLink.Title.CallerFilePath}:{uowLink.Title.CallerLineNumber})\nSessionId {sessionId}");
				UowRegistered?.Invoke(null, new UowRegistereEventArgs(uow));
			}
		}

		public static int GetActiveUoWCount()
		{
			return RegisteredUoWs.Count;
		}

		internal static void UnregisterUow(IUnitOfWorkTracked uow)
		{
			RemoveLost();
			lock (RegisteredUoWs)
			{
				var createdString = $"{uow.ActionTitle.CallerMemberName} ({uow.ActionTitle.CallerFilePath}:{uow.ActionTitle.CallerLineNumber})";
				var sessionId = (uow.Session as ISessionImplementor).SessionId;
				RegisteredUoWs.Remove(sessionId);
				logger.Debug($"UnitOfWork, созданный в {createdString}, завершил работу. SessionId {sessionId}\n{ActiveUowCountText()}");
			}
		}

		private static string ActiveUowCountText()
		{
			return NumberToTextRus.FormatCase(RegisteredUoWs.Count, "Сейчас {0} активный UnitOfWork.", "Сейчас {0} активных UnitOfWork.", "Сейчас {0} активных UnitOfWork.");
		}

		public static IUnitOfWorkTracked GetUnitOfWorkTracked(ISessionImplementor session)
		{
			lock (RegisteredUoWs)
			{
				if(RegisteredUoWs.ContainsKey(session.SessionId))
				{
					return RegisteredUoWs[session.SessionId].UnitOfWork;
				}
			}
			return null;
		}

		public static IUnitOfWork GetUnitOfWork(ISessionImplementor session)
		{
			return GetUnitOfWorkTracked(session) as IUnitOfWork;
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
						logger.Warn($"UnitOfWork созданный в {pair.Value.Title.CallerMemberName}({pair.Value.Title.CallerFilePath}:{pair.Value.Title.CallerLineNumber})" +
							$" не был закрыт корректно и удален сборщиком мусора так как на него потеряны все ссылки." +
							$"\n SessionId {pair.Key}");
						RegisteredUoWs.Remove(pair.Key);
					}
				}

				nextCheck = DateTime.Now.AddSeconds(5);
			}
		}
	}

	[System.Serializable]
	public sealed class UowRegistereEventArgs : EventArgs
	{
		public IUnitOfWorkTracked UoW { get; private set; }

		public UowRegistereEventArgs(IUnitOfWorkTracked uow)
		{
			UoW = uow;
		}
	}
}
