using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Event;
using NHibernate.Proxy;
using QS.DomainModel.Tracking;
using QS.HistoryLog.Domain;
using QS.Project.Repositories;
using QSOrmProject;
using QSProjectsLib;

namespace QS.HistoryLog
{
	public class HibernateTracker : IHibernateTracker
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		readonly List<ChangedEntity> changes = new List<ChangedEntity>();

		public HibernateTracker()
		{
		}

		public void OnPostDelete(PostDeleteEvent deleteEvent)
		{
			var entity = deleteEvent.Entity as IDomainObject;

			var type = NHibernateProxyHelper.GuessClass(entity);
			if(!TrackerMain.Factory?.NeedTrace(type) ?? false)
				return;

			if(changes.Exists(di => di.Operation == EntityChangeOperation.Delete && di.EntityClassName == type.Name && di.EntityId == entity.Id))
				return;

			var hce = new ChangedEntity(EntityChangeOperation.Delete, entity);
			changes.Add(hce);
		}

		public void OnPostUpdate(PostUpdateEvent updateEvent)
		{
			var entity = updateEvent.Entity as IDomainObject;
			// Мы умеет трекать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !TrackerMain.NeedTrace(entity))
				return;

			var fields = Enumerable.Range(0, updateEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(i, updateEvent))
								   .Where(x => x != null)
								   .ToList();

			if(fields.Count > 0)
			{
				var hce = new ChangedEntity(EntityChangeOperation.Change, updateEvent.Entity);
				fields.ForEach(f => f.Entity = hce);
				hce.Changes = fields;
				changes.Add(hce);
			}
		}

		public void Reset()
		{
			changes.Clear();
		}

		/// <summary>
		/// Сохраняем журнал изменений через новый UnitOfWork.
		/// </summary>
		public void SaveChangeSet(IUnitOfWork userUoW)
		{
			if(changes.Count == 0)
				return;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var user = UserRepository.GetCurrentUser(uow);

				var conStr = userUoW.Session.Connection.ConnectionString;
				var reg = new Regex("user id=(.+?)(;|$)");
				var match = reg.Match(conStr);
				string dbLogin = match.Success ? match.Groups[0].Value : null;

				var changeset = new ChangeSet(userUoW.ActionName, user, dbLogin);
				changeset.AddChange(changes.ToArray());
				uow.Save(changeset);
				uow.Commit();
				logger.Debug(RusNumber.FormatCase(changes.Count, "Зарегистрировано изменение {0} объекта.", "Зарегистрировано изменение {0} объектов.", "Зарегистрировано изменение {0} объектов."));

				Reset();
			}
		}
	}
}
