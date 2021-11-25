using System;
using System.Collections;
using System.Linq;
using NHibernate.Criterion;
using QS.Deletion.Configuration;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Deletion
{
	/// <summary>
	/// Класс позволяет заменять ссылки в базе с одной сущьности на другую.
	/// Поиск зависимых объектов осуществляется на основании конфигурации удаления.
	/// </summary>
	public class ReplaceEntity
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly DeleteConfiguration configuration;

		public IProgressBarDisplayable Progress;

		public ReplaceEntity(DeleteConfiguration configuration)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public int ReplaceEverywhere<TEntity>(IUnitOfWork uow, TEntity fromE, TEntity toE)
			where TEntity : IDomainObject
		{
			int replacedLinks = 0;

			if(fromE == null)
				throw new ArgumentNullException(nameof(fromE));
			if(toE == null)
				throw new ArgumentNullException(nameof(toE));
			if(fromE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(fromE));
			if(toE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(toE));

			var delConfig = configuration.GetDeleteInfo<TEntity>();
			if(delConfig == null)
				throw new InvalidOperationException($"Конфигурация удаления для типа {typeof(TEntity)} не найдена.");

			if( !(delConfig is IDeleteInfoHibernate))
				throw new NotSupportedException($"Поддерживаются только конфигурации удаляения Hibernate.");

			Progress?.Start(delConfig.DeleteItems.Count + delConfig.ClearItems.Count + delConfig.RemoveFromItems.Count);

			foreach(var depend in delConfig.DeleteItems)
			{
				Progress?.Add();
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
				Progress?.Add();
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				foreach(var item in GetLinkedEntities(uow, depend, fromE)) {
					propInfo.SetValue(item, toE, null);
					uow.TrySave(item);
					replacedLinks++;
				}
			}

			foreach(var depend in delConfig.RemoveFromItems) {
				Progress?.Add();
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

			Progress?.Close();
			return replacedLinks;
		}

		public int CalculateTotalLinks<TEntity>(IUnitOfWork uow, TEntity fromE)
			where TEntity : IDomainObject
		{
			logger.Info("Подсчет ссылок...");
			int totalLinks = 0;

			if(fromE == null)
				throw new ArgumentNullException(nameof(fromE));
			if(fromE.Id == 0)
				throw new ArgumentException("Сущьность должна уже иметь ID", nameof(fromE));

			var delConfig = configuration.GetDeleteInfo<TEntity>();
			if(delConfig == null)
				throw new InvalidOperationException($"Конфигурация удаления для типа {typeof(TEntity)} не найдена.");

			if(!(delConfig is IDeleteInfoHibernate))
				throw new NotSupportedException($"Поддерживаются только конфигурации удаляения Hibernate.");
			
			Progress?.Start(delConfig.DeleteItems.Count + delConfig.ClearItems.Count + delConfig.RemoveFromItems.Count);

			foreach(var depend in delConfig.DeleteItems) {
				Progress?.Add();
				if(String.IsNullOrEmpty(depend.PropertyName))
					continue;
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			foreach(var depend in delConfig.ClearItems) {
				Progress?.Add();
				var propInfo = depend.ObjectClass.GetProperty(depend.PropertyName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			foreach(var depend in delConfig.RemoveFromItems) {
				Progress?.Add();
				var collPropInfo = depend.ObjectClass.GetProperty(depend.CollectionName);
				totalLinks += GetLinkedEntities(uow, depend, fromE).Count;
			}

			logger.Info("Найдено {0} ссылок.", totalLinks);
			Progress?.Close();
			return totalLinks;
		}

		private IList GetLinkedEntities(IUnitOfWork uow, DeleteDependenceInfo depend, IDomainObject masterEntity)
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

		private IList GetLinkedEntities(IUnitOfWork uow, RemoveFromDependenceInfo depend, IDomainObject masterEntity)
		{
			var list = uow.Session.CreateCriteria(depend.ObjectClass)
				.CreateAlias(depend.CollectionName, "childs")
				.Add(Restrictions.Eq(String.Format("childs.Id", depend.CollectionName), masterEntity.Id)).List();

			return list;
		}

		private IList GetLinkedEntities(IUnitOfWork uow, ClearDependenceInfo depend, IDomainObject masterEntity)
		{
			var list = uow.Session.CreateCriteria(depend.ObjectClass)
				.Add(Restrictions.Eq(depend.PropertyName + ".Id", masterEntity.Id)).List();

			return list;
		}
	}
}
