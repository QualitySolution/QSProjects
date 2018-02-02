using System;
using System.Collections;
using System.Linq;
using NHibernate.Criterion;

namespace QSOrmProject.Deletion
{
	public static class ReplaceEntity
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public static int ReplaceEverywhere<TEntity>(IUnitOfWork uow, TEntity fromE, TEntity toE)
			where TEntity : IDomainObject
		{
			int replacedLinks = 0;

			if(fromE == null || toE == null)
				throw new ArgumentNullException("fromE || toE");
			if(fromE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(fromE));
			if(toE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(toE));

			var delConfig = DeleteConfig.GetDeleteInfo<TEntity>();
			if(delConfig == null)
				throw new InvalidOperationException($"Конфигурация удаления для типа {typeof(TEntity)} не найдена.");

			if( !(delConfig is IDeleteInfoHibernate))
				throw new NotSupportedException($"Поддерживаются только конфигурации удаляения Hibernate.");

			foreach(var depend in delConfig.DeleteItems)
			{
				if(String.IsNullOrEmpty(depend.PropertyName))
					continue;
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				foreach(var item in GetLinkedEntities(uow, depend, fromE))
				{
					propInfo.SetValue(item, toE, null);
					uow.TrySave(item);
					replacedLinks++;
				}
			}

			foreach(var depend in delConfig.ClearItems) {
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				foreach(var item in GetLinkedEntities(uow, depend, fromE)) {
					propInfo.SetValue(item, toE, null);
					uow.TrySave(item);
					replacedLinks++;
				}
			}

			foreach(var depend in delConfig.RemoveFromItems) {
				var collPropInfo = depend.ObjectClass.GetProperty(depend.CollectionName);
				foreach(var item in GetLinkedEntities(uow, depend, fromE)) {
					var coll = (collPropInfo as IList);
					var replaced = coll.Cast<IDomainObject>().First(x => x.Id == fromE.Id);
					var ix = coll.IndexOf(replaced);
					collPropInfo.SetValue(item, toE, new object[] {ix});
					uow.TrySave(item);
					replacedLinks++;
				}
			}

			return replacedLinks;
		}

		public static int CalculateTotalLinks<TEntity>(IUnitOfWork uow, TEntity fromE)
	where TEntity : IDomainObject
		{
			logger.Info("Подсчет ссылок...");
			int totalLinks = 0;

			if(fromE == null)
				throw new ArgumentNullException("fromE");
			if(fromE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(fromE));

			var delConfig = DeleteConfig.GetDeleteInfo<TEntity>();
			if(delConfig == null)
				throw new InvalidOperationException($"Конфигурация удаления для типа {typeof(TEntity)} не найдена.");

			if(!(delConfig is IDeleteInfoHibernate))
				throw new NotSupportedException($"Поддерживаются только конфигурации удаляения Hibernate.");

			foreach(var depend in delConfig.DeleteItems) {
				if(String.IsNullOrEmpty(depend.PropertyName))
					continue;
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			foreach(var depend in delConfig.ClearItems) {
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			foreach(var depend in delConfig.RemoveFromItems) {
				var collPropInfo = depend.ObjectClass.GetProperty(depend.CollectionName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			logger.Info("Найдено {0} ссылок.", totalLinks);
			return totalLinks;
		}

		private static IList GetLinkedEntities(IUnitOfWork uow, DeleteDependenceInfo depend, IDomainObject masterEntity)
		{
			if(depend.PropertyName != null) {
				var list = uow.Session.CreateCriteria(depend.ObjectClass)
					.Add(Restrictions.Eq(depend.PropertyName + ".Id", masterEntity.Id)).List();

				return list;
			} else if(depend.CollectionName != null) {
				//CheckAndLoadEntity(core, masterEntity);
				//return MakeResultList(
				//	masterEntity.Entity.GetPropertyValue(depend.CollectionName) as IList);
			} else if(depend.ParentPropertyName != null) {
				//CheckAndLoadEntity(core, masterEntity);
				//var value = (TEntity)masterEntity.Entity.GetPropertyValue(depend.ParentPropertyName);

				//return MakeResultList(value == null ? new List<TEntity>() : new List<TEntity> { value });
			}

			throw new NotImplementedException();
		}

		private static IList GetLinkedEntities(IUnitOfWork uow, RemoveFromDependenceInfo depend, IDomainObject masterEntity)
		{
			var list = uow.Session.CreateCriteria(depend.ObjectClass)
				.CreateAlias(depend.CollectionName, "childs")
				.Add(Restrictions.Eq(String.Format("childs.Id", depend.CollectionName), masterEntity.Id)).List();

			return list;
		}

		private static IList GetLinkedEntities(IUnitOfWork uow, ClearDependenceInfo depend, IDomainObject masterEntity)
		{
			var list = uow.Session.CreateCriteria(depend.ObjectClass)
				.Add(Restrictions.Eq(depend.PropertyName + ".Id", masterEntity.Id)).List();

			return list;
		}

	}
}
