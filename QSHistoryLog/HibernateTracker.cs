using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Event;
using NHibernate.Proxy;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using QS.Project.Repositories;
using QS.Utilities;
using QSOrmProject;

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

			var hce = new ChangedEntity(EntityChangeOperation.Delete, entity, new List<FieldChange>());
			changes.Add(hce);
		}

		public void OnPostInsert(PostInsertEvent insertEvent)
		{
			var entity = insertEvent.Entity as IDomainObject;
			// Мы умеет трекать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !TrackerMain.NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id && NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName))
				return;

			var fields = Enumerable.Range(0, insertEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(i, insertEvent))
								   .Where(x => x != null)
								   .ToList();

			if(fields.Count > 0) {
				changes.Add(new ChangedEntity(EntityChangeOperation.Create, insertEvent.Entity, fields));
			}
		}

		public void OnPostUpdate(PostUpdateEvent updateEvent)
		{
			var entity = updateEvent.Entity as IDomainObject;
			// Мы умеет трекать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !TrackerMain.NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id && NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName))
				return;

			var fields = Enumerable.Range(0, updateEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(i, updateEvent))
								   .Where(x => x != null)
								   .ToList();

			if(fields.Count > 0)
			{
				changes.Add(new ChangedEntity(EntityChangeOperation.Change, updateEvent.Entity, fields));
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
				var changeset = new ChangeSet(userUoW.ActionTitle?.UserActionTitle ?? userUoW.ActionTitle?.CallerMemberName, user, dbLogin);
				changeset.AddChange(changes.ToArray());
				uow.Save(changeset);
				uow.Commit();
				logger.Debug(NumberToTextRus.FormatCase(changes.Count, "Зарегистрировано изменение {0} объекта.", "Зарегистрировано изменение {0} объектов.", "Зарегистрировано изменение {0} объектов."));

				Reset();
			}
		}
	}
}
