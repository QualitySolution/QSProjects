using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Utilities;

namespace QS.DomainModel.Tracking
{
	public static class UowWatcher
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		internal static readonly Dictionary<int, UowLink> RegisteredUoWs = new Dictionary<int, UowLink>();

		public static event EventHandler<UowRegistereEventArgs> UowRegistered;

		internal static void RegisterUow(IUnitOfWorkTracked uow)
		{
			RemoveLost();
			lock (RegisteredUoWs)
			{
				var uowLink = new UowLink(uow);
				RegisteredUoWs.Add(uow.Session.GetHashCode(), uowLink);
				logger.Debug($"Зарегистрирован новый UnitOfWork. {ActiveUowCountText()}. Создан в {uowLink.Title.CallerMemberName} ({uowLink.Title.CallerFilePath}:{uowLink.Title.CallerLineNumber})");
				UowRegistered?.Invoke(null, new UowRegistereEventArgs(uow));
			}
		}

		internal static void UnregisterUow(IUnitOfWorkTracked uow)
		{
			RemoveLost();
			lock (RegisteredUoWs)
			{
				var uowLink = RegisteredUoWs.FirstOrDefault(x => x.Value.UnitOfWork == uow).Value;
				var createdString = $"Создан в {uowLink.Title.CallerMemberName} ({uowLink.Title.CallerFilePath}:{uowLink.Title.CallerLineNumber})";
				RegisteredUoWs.Remove(uow.Session.GetHashCode());
				logger.Debug("UnitOfWork завершил работу. {0}. {1}.", createdString, ActiveUowCountText());
			}
		}

		private static string ActiveUowCountText()
		{
			return NumberToTextRus.FormatCase(RegisteredUoWs.Count, "Сейчас {0} активный UnitOfWork.", "Сейчас {0} активных UnitOfWork.", "Сейчас {0} активных UnitOfWork.");
		}

		public static IUnitOfWorkTracked GetUnitOfWorkTracked(NHibernate.ISession session)
		{
			lock (RegisteredUoWs)
			{
				int hashCode = session.GetHashCode();
				if (RegisteredUoWs.ContainsKey(hashCode))
				{
					return RegisteredUoWs[hashCode].UnitOfWork;
				}
			}
			return null;
		}

		public static IUnitOfWork GetUnitOfWork(NHibernate.ISession session)
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
						logger.Warn($"UnitOfWork созданный в {pair.Value.Title.CallerMemberName}({pair.Value.Title.CallerFilePath}:{pair.Value.Title.CallerLineNumber}) не был закрыт корректно и удален сборщиком мусора так как на него потеряны все ссылки.");
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
