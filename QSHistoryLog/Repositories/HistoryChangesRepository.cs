using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;

namespace QS.HistoryLog.Repositories
{
	public static class HistoryChangesRepository
	{
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
	}
}
