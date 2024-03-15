using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MySqlConnector;
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
			// Мы умеет трекать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id && NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName))
				return;

			var fields = Enumerable.Range(0, insertEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(uow, i, insertEvent))
								   .Where(x => x != null)
								   .ToList();

			if(fields.Count > 0) {
				changes.Add(new ChangedEntity(EntityChangeOperation.Create, insertEvent.Entity, fields));
			}
		}

		public void OnPostUpdate(IUnitOfWorkTracked uow, PostUpdateEvent updateEvent)
		{
			var entity = updateEvent.Entity as IDomainObject;
			// Мы умеет трекать только объекты реализующие IDomainObject, иначе далее будем падать на получении Id.
			if(entity == null || !NeedTrace(entity))
				return;

			//FIXME добавлено чтобы не дублировались записи. Потому что от Nhibernate приходит по 2 события на один объект. Если это удастся починить, то этот код не нужен.
			if(changes.Any(hce => hce.EntityId == entity.Id && NHibernateProxyHelper.GuessClass(entity).Name == hce.EntityClassName))
				return;

			var fields = Enumerable.Range(0, updateEvent.State.Length)
								   .Select(i => FieldChange.CheckChange(uow, i, updateEvent))
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

		public void OnPostCommit(IUnitOfWorkTracked uow)
		{
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
			//из-за таких действий история изменений для пользователя выглядит странно. Код ниже удаляет бестолковые записи об изменениях. 
			var hashOfDeleted = new HashSet<string>(
				changes.Where(x => x.Operation == EntityChangeOperation.Delete)
					.Select(x => x.EntityHash)
				);
			var toSave = changes.Where(x => x.Operation == EntityChangeOperation.Delete || !hashOfDeleted.Contains(x.EntityHash));	

			changeSet.AddChangeEntities(toSave);
			
			Save(changeSet);
			logger.Debug(NumberToTextRus.FormatCase(changes.Sum(x => x.Changes.Count), "Зарегистрировано {0} изменение ",
				             "Зарегистрировано {0} изменения ", "Зарегистрировано {0} изменений ")
			             + NumberToTextRus.FormatCase(changes.Count, "в {0} объекте ", "в {0} объектах ", "в {0} объектах ")
			             + $"за {(DateTime.Now-start).TotalSeconds} сек.");
			Reset();
		}

		private void Save(ChangeSet changeSet) {
			using(var connection = new MySqlConnection(_connectionStringBuilder.ConnectionString)) {
				connection.Open();
				var transaction = connection.BeginTransaction();
				using(var batch = new MySqlBatch(connection, transaction))
				{
					var sqlInsertChangeSet =
						"INSERT INTO history_changeset (user_login, action_name, user_id) " +
						"VALUES (@UserLogin, @ActionName, @UserId);";
					batch.BatchCommands.Add(new MySqlBatchCommand(sqlInsertChangeSet) {
						Parameters = {
							new MySqlParameter("@UserLogin", changeSet.UserLogin),
							new MySqlParameter("@ActionName", changeSet.ActionName),
							new MySqlParameter("@UserId", changeSet.UserId),
						}
					});
					
					batch.BatchCommands.Add(new MySqlBatchCommand("SET @ChangeSetId = LAST_INSERT_ID();"));
					
					var sqlInsertEntity =
						"INSERT INTO history_changed_entities (datetime, operation, entity_class, entity_id, entity_title, changeset_id) " +
						"VALUES (@ChangeTime, @OperationDbName, @EntityClassName, @EntityId, @EntityTitle, @ChangeSetId);";
					
					var sqlSetChangedEntityId = "SET @ChangedEntityId = LAST_INSERT_ID();";
					
					var sqlInsertChange = 
						"INSERT INTO history_changes (type, field_name, old_value, old_id, new_value, new_id, changed_entity_id) " +
					    "VALUES (@TypeOfChange, @Path, @OldValue, @OldId, @NewValue, @NewId, @ChangedEntityId);";

					foreach(var entity in changeSet.Entities) {
						batch.BatchCommands.Add(new MySqlBatchCommand(sqlInsertEntity) {
							Parameters = {
								new MySqlParameter("@ChangeTime", entity.ChangeTime),
								new MySqlParameter("@OperationDbName", entity.Operation.ToString()),
								new MySqlParameter("@EntityClassName", entity.EntityClassName),
								new MySqlParameter("@EntityId", entity.EntityId),
								new MySqlParameter("@EntityTitle", entity.EntityTitle),
							}
						});
						batch.BatchCommands.Add(new MySqlBatchCommand(sqlSetChangedEntityId));
						foreach(var change in entity.Changes) {
							batch.BatchCommands.Add(new MySqlBatchCommand(sqlInsertChange) {
									Parameters = {
										new MySqlParameter("@TypeOfChange", change.Type.ToString()),
										new MySqlParameter("@Path", change.Path),
										new MySqlParameter("@OldValue", change.OldValue),
										new MySqlParameter("@OldId", change.OldId),
										new MySqlParameter("@NewValue", change.NewValue),
										new MySqlParameter("@NewId", change.NewId),
									}
								}
							);
						}
					}
					batch.ExecuteNonQuery();
				};
				transaction.Commit();
			}
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
