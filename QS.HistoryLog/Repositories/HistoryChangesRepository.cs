using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog.Repositories
{
	public static class HistoryChangesRepository
	{
		private const string changedEntityAlias = "hce";
		private const string changeSetAlias = "hcs";
		private const string fieldChangeAlias = "hc";

		/// <summary>
		/// Возвращает набор изменений объектов по их id
		/// </summary>
		public static IList<ChangedEntity> GetHistoryChanges<T>(IUnitOfWork uow, int[] idList)
			where T : IDomainObject
		{
			var changes = uow.Session.QueryOver<ChangedEntity>()
			                 .Where(x => x.EntityClassName == typeof(T).Name)
			                 .WhereRestrictionOn(x => x.EntityId).IsIn(idList)
			                 .List();
			return changes;
		}

		/// <summary>
		/// Возвращает только изменения конкретных полей объектов
		/// </summary>
		public static IList<FieldChange> GetFieldChanges<T>(IUnitOfWork uow, int[] entitiesIds, Expression<Func<T, object>> field)
			where T : IDomainObject
		{
			ChangedEntity changedEntityAlias = null;
			var prop = Gamma.Utilities.PropertyUtil.GetName(field);

			var changes = uow.Session.QueryOver<FieldChange>()
			                 .Left.JoinAlias(fc => fc.Entity, () => changedEntityAlias)
			                 .Where(() => changedEntityAlias.EntityClassName == typeof(T).Name)
			                 .Where(() => changedEntityAlias.EntityId.IsIn(entitiesIds))
							 .Where(fc => fc.Path == prop)
							 .List();
			return changes;
		}
		
		/// <summary>
		/// Возвращает последнюю запись до указанного периода от сегодняшнего дня
		/// </summary>
		public static ChangedEntity GetChangedEntity(IUnitOfWork uow, int days)
		{
			return uow.Session.QueryOver<ChangedEntity>()
				.Where(ce => ce.ChangeTime < DateTime.Today.AddDays(-days))
				.OrderBy(ce => ce.Id).Desc
				.Take(1)
				.SingleOrDefault();
		}

		/// <summary>
		/// Возвращает true, если есть изменения за указанный период, иначе false
		/// </summary>
		public static bool ChangedEntitiesExists(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo) {
			return uow.Session.QueryOver<ChangedEntity>()
				.Where(Restrictions.Between(Projections.Property<ChangedEntity>(ce => ce.ChangeTime), dateFrom, dateTo))
				.List()
				.Any();
		}

		public static ChangedEntity GetFirstChangedEntity(IUnitOfWork uow) {
			return uow.Session.QueryOver<ChangedEntity>()
				.Take(1)
				.SingleOrDefault();
		}

		public static void DeleteHistoryChanges(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo) {
			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var hcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangeSet));
			var hcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(FieldChange));

			var changeSetColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeSet)).First();
			var changeTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();
			var changedEntityColumn = hcPersister.GetPropertyColumnNames(nameof(FieldChange.Entity)).First();

			var query = $"DELETE {fieldChangeAlias}, {changedEntityAlias}, {changeSetAlias} "
				+ $"FROM {hcePersister.TableName} AS {changedEntityAlias} "
				+ $"JOIN {hcsPersister.TableName} AS {changeSetAlias} ON {changeSetAlias}.{hcsPersister.KeyColumnNames.First()} = {changedEntityAlias}.{changeSetColumn} "
				+ $"LEFT JOIN {hcPersister.TableName} AS {fieldChangeAlias} ON {changedEntityAlias}.{hcePersister.KeyColumnNames.First()} = {fieldChangeAlias}.{changedEntityColumn} "
				+ $"WHERE {changedEntityAlias}.{changeTimeColumn} BETWEEN '{dateFrom:yyyy-MM-dd}' AND '{dateTo:yyyy-MM-dd HH:mm:ss}';";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}
	}
}
