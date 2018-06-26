using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Event;
using NHibernate.Proxy;
using QS.DomainModel.Tracking;
using QSHistoryLog.Domain;
using QSOrmProject;
using QSProjectsLib;

namespace QSHistoryLog
{
	public class HibernateTracker : IHibernateTracker
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		readonly List<HistoryChangeSet> changes = new List<HistoryChangeSet>();

		public HibernateTracker()
		{
		}

		public void OnPostDelete(PostDeleteEvent deleteEvent)
		{
			var entity = deleteEvent.Entity as IDomainObject;

			var type = NHibernateProxyHelper.GuessClass(entity);
			if(!TrackerMain.Factory?.NeedTrace(type) ?? false)
				return;

			if(changes.Exists(di => di.Operation == ChangeSetType.Delete && di.ObjectName == type.Name && di.ItemId == entity.Id))
				return;

			var title = DomainHelper.GetObjectTilte(entity);
			var hcs = new HistoryChangeSet(ChangeSetType.Delete, type, entity.Id, title);
			changes.Add(hcs);
		}

		public void OnPostUpdate(PostUpdateEvent updateEvent)
		{
			var fields = Enumerable.Range(0, updateEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(i, updateEvent))
								   .Where(x => x != null)
								   .ToList();

			if(fields.Count > 0)
			{
				var type = NHibernateProxyHelper.GuessClass(updateEvent.Entity);
				var hcs = new HistoryChangeSet(ChangeSetType.Change, type, (int)updateEvent.Id, DomainHelper.GetObjectTilte(updateEvent.Entity));
				fields.ForEach(f => f.ChangeSet = hcs);
				hcs.Changes = fields;
				changes.Add(hcs);
			}
		}

		public void Reset()
		{
			changes.Clear();
		}

		/// <summary>
		/// Сохраняем в своей сессии.
		/// </summary>
		public void SaveChangeSet()
		{
			if(changes.Count == 0)
				return;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				changes.ForEach(hcs => uow.Save(hcs));
				logger.Debug(RusNumber.FormatCase(changes.Count, "Зарегистрировано изменение {0} объекта.", "Зарегистрировано изменение {0} объектов.", "Зарегистрировано изменение {0} объектов."));
				uow.Commit();
				Reset();
			}
		}
	}
}
