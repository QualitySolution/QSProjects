using System;
using System.Collections.Generic;
using NHibernate.Proxy;
using QS.DomainModel.Tracking;
using QSHistoryLog.Domain;
using QSOrmProject;
using QSProjectsLib;

namespace QSHistoryLog
{
	public class DeleteTracker : IDeleteTracker
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		readonly List<HistoryChangeSet> deleteItems = new List<HistoryChangeSet>();

		public DeleteTracker()
		{
		}

		public void MarkDeleted(IDomainObject entity)
		{
			var type = NHibernateProxyHelper.GuessClass(entity);
			if(!TrackerMain.Factory?.NeedTrace(type) ?? false)
				return;

			if(deleteItems.Exists(di => di.ObjectName == type.Name && di.ItemId == entity.Id))
				return;

			var title = DomainHelper.GetObjectTilte(entity);
			var hcs = new HistoryChangeSet(ChangeSetType.Delete, type, entity.Id, title);
			deleteItems.Add(hcs);
		}

		public void Reset()
		{
			deleteItems.Clear();
		}

		/// <summary>
		/// Сохраняем в своей сессии.
		/// </summary>
		public void SaveChangeSet()
		{
			if(deleteItems.Count == 0)
				return;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				deleteItems.ForEach(hcs => uow.Save(hcs));
				logger.Debug(RusNumber.FormatCase(deleteItems.Count, "Отследили {0} удаление", "Вычислили {0} удаления", "Проследили за {0} удалениями"));
				uow.Commit();
				Reset();
			}
		}
	}
}
