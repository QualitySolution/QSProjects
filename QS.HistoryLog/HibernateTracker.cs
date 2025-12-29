using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace QS.HistoryLog
{
	public class HibernateTracker : ISingleUowEventListener, IUowPostInsertEventListener, IUowPostUpdateEventListener, IUowPostDeleteEventListener, IUowPostCommitEventListener {
		/// <summary>
		/// Используется для записи журнала изменений.
		/// </summary>
		private readonly MySqlConnectionStringBuilder _connectionStringBuilder;
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private static ReadOnlyCollection<char> DIRECTORY_SEPARATORS = new ReadOnlyCollection<char>(new List<char>() { '\\', '/' });

		readonly List<ChangedEntity> changes = new List<ChangedEntity>();

		public HibernateTracker(MySqlConnectionStringBuilder connectionStringBuilder) {
			if(!connectionStringBuilder.AllowUserVariables) {
				connectionStringBuilder.AllowUserVariables = true;
			}
			
			_connectionStringBuilder = connectionStringBuilder;
		}

		public void OnPostDelete(IUnitOfWorkTracked uow, PostDeleteEvent deleteEvent)
		{
			var entity = deleteEvent.Entity as IDomainObject;

			var type = NHibernateProxyHelper.GuessClass(entity);
			if(!NeedTrace(type))
				return;

			if(changes.Exists(di => di.Operation == EntityChangeOperation.Delete && di.EntityClassName == type.Name && di.EntityId == entity.Id))
				return;

			var hce = new ChangedEntity(EntityChangeOperation.Delete, entity, new List<FieldChange>());
			changes.Add(hce);
		}

		public void OnPostInsert(IUnitOfWorkTracked uow, PostInsertEvent insertEvent)
		{
			var entity = insertEvent.Entity as IDomainObject;
			// Умеем отслеживать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id
				&& NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName
				&& hce.Operation == EntityChangeOperation.Create))
				return;

			List<FieldChange> fields = new List<FieldChange>(Enumerable.Range(0, insertEvent.State.Length)
				.Select(i => FieldChange.CheckChange(uow, i, insertEvent))
				.Where(x => x != null));

			if(fields.Any()) {
				changes.Add(new ChangedEntity(EntityChangeOperation.Create, insertEvent.Entity, fields));
			}
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent) {
			var entity = updateEvent.Entity as IDomainObject;
			// Умеем отслеживать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id
				&& NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName
				&& hce.Operation == EntityChangeOperation.Change))
				return;

			List<FieldChange> fields = new List<FieldChange>(Enumerable.Range(0, updateEvent.State.Length)
				.Select(i => FieldChange.CheckChange(uow, i, updateEvent))
				.Where(x => x != null));

			if(fields.Any())
				changes.Add(new ChangedEntity(EntityChangeOperation.Change, updateEvent.Entity, fields));
		}

		public void Reset() {
			changes.Clear();
		}

		public void OnPostCommit(IUnitOfWorkTracked uow) {
			SaveChangeSet((IUnitOfWork)uow);
		}

		/// <summary>
		/// Сохраняем журнал изменений через новый UnitOfWork.
		/// </summary>
		public void SaveChangeSet(IUnitOfWork userUoW)
		{
			if(changes.Count == 0)
				return;

			var userId = UserRepository.GetCurrentUserId();
			var start = DateTime.Now;
			var dbLogin = _connectionStringBuilder.UserID;

			var changeSet = new ChangeSet(userUoW.ActionTitle?.UserActionTitle 
					?? userUoW.ActionTitle?.CallerMemberName + " - " 
						+ userUoW.ActionTitle.CallerFilePath.Substring(userUoW.ActionTitle.CallerFilePath.LastIndexOfAny(DIRECTORY_SEPARATORS.ToArray()) + 1)
						+ " (" + userUoW.ActionTitle.CallerLineNumber + ")",
				userId,
				dbLogin);
			
			//NHibernate очень часто при удалении множества объектов, имеющих ссылки друг на друга, сначала очищает у объекта поля со ссылками
			//на другой удаляемый объект, а потом его удалят тот в котором очищались ссылки. Что достаточно бестолково, можно было просто удалить.
			//Из-за таких действий история изменений для пользователя выглядит странно. Код ниже удаляет бестолковые записи об изменениях. 
			var hashOfDeleted = new HashSet<string>(
				changes.Where(x => x.Operation == EntityChangeOperation.Delete)
					.Select(x => x.EntityHash)
				);
			var toSave = changes.Where(x => x.Operation == EntityChangeOperation.Delete || !hashOfDeleted.Contains(x.EntityHash));	

			changeSet.AddChangeEntities(toSave);

			new ChangeSetWriter(connectionString).Save(changeSet);
			logger.Debug(NumberToTextRus.FormatCase(changes.Sum(x => x.Changes.Count), "Зарегистрировано {0} изменение ",
				             "Зарегистрировано {0} изменения ", "Зарегистрировано {0} изменений ")
			             + NumberToTextRus.FormatCase(changes.Count, "в {0} объекте ", "в {0} объектах ", "в {0} объектах ")
			             + $"за {(DateTime.Now-start).TotalSeconds} сек.");
			Reset();
		}

		#region Проверка нужно ли записывать изменения

		public static bool NeedTrace(IDomainObject entity)
		{
			var type = NHibernateProxyHelper.GuessClass(entity);
			return NeedTrace(type);
		}

		public static bool NeedTrace(Type entityClass)
		{
			if (entityClass.GetInterface(typeof(NHibernate.Proxy.DynamicProxy.IProxy).FullName) != null || entityClass.GetInterface(typeof(INHibernateProxy).FullName) != null)
				entityClass = entityClass.BaseType;

			return entityClass.GetCustomAttributes(typeof(HistoryTraceAttribute), true).Length > 0;
		}

		#endregion
	}
}
