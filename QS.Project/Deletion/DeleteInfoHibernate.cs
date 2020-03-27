using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.Deletion
{
	public class DeleteInfoHibernate<TEntity> : IDeleteInfoHibernate, IHibernateDeleteRule
		where TEntity : IDomainObject
	{
		#region Свойства

		public Type ObjectClass {
			get {
				return typeof(TEntity);
			}
		}

		//Только для поиска соответствия из модулей работающих по таблицам. Например Users в QSProjectLib
		public string TableName { get; private set;}
			
		public string ObjectsName {
			get {
				var att = ObjectClass.GetCustomAttributes (typeof(AppellativeAttribute), true);
				if (att.Length > 0)
					return (att [0] as AppellativeAttribute).NominativePlural;
				else
					return null;
			}
		}

		public bool IsRootForSubclasses { get; set;}

		public bool IsRequiredCascadeDeletion { get; set;}

		public bool IsSubclass
		{
			get
			{
				return OrmConfig.NhConfig.GetClassMapping(ObjectClass) is NHibernate.Mapping.Subclass;
			}
		}

		public bool HasDependences
		{
			get
			{
				return DeleteItems.Count > 0 || ClearItems.Count > 0 || RemoveFromItems.Count > 0 || IsRootForSubclasses;
			}
		}

		public List<DeleteDependenceInfo> DeleteItems { get; set;}
		public List<ClearDependenceInfo> ClearItems { get; set;}
		public List<RemoveFromDependenceInfo> RemoveFromItems { get; set;}

		#endregion

		public DeleteInfoHibernate()
		{
			DeleteItems = new List<DeleteDependenceInfo>();
			ClearItems = new List<ClearDependenceInfo>();
			RemoveFromItems = new List<RemoveFromDependenceInfo>();

			var hmap = OrmConfig.NhConfig.GetClassMapping(ObjectClass);
			if (hmap == null)
				throw new InvalidOperationException(String.Format("Класс {0} отсутствует в мапинге NHibernate.", ObjectClass));
			TableName = hmap.Table.Name;
		}

		#region Fluent Config

		public DeleteInfoHibernate<TEntity> AddDeleteDependence (DeleteDependenceInfo info)
		{
			DeleteItems.Add (info);
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddDeleteDependence<TDependOn>(Expression<Func<TDependOn, object>> propertyRefExpr)
		{
			DeleteItems.Add (DeleteDependenceInfo.Create<TDependOn> (propertyRefExpr));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddDeleteCascadeDependence(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			DeleteItems.Add (DeleteDependenceInfo.CreateFromParentPropery<TEntity> (propertyRefExpr));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddRemoveFromDependence<TFrom>(Expression<Func<TFrom, object>> collectionProperty)
		{
			RemoveFromItems.Add (RemoveFromDependenceInfo.CreateFromBag <TFrom> (collectionProperty));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddRemoveFromDependence<TFrom>(Expression<Func<TFrom, object>> collectionProperty, Expression<Func<TFrom, Action<TEntity>>> removeFunction)
		{
			RemoveFromItems.Add (RemoveFromDependenceInfo.CreateFromBag <TFrom, TEntity> (collectionProperty, removeFunction));
			return this;
		}

		//TODO удалить метод после 11.2019;
		[Obsolete("Используйте вместо этого более универсальный метод AddDeleteDependenceFromCollection.")]
		public DeleteInfoHibernate<TEntity> AddDeleteDependenceFromBag(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			string propName = PropertyUtil.GetName(propertyRefExpr);
			DeleteItems.Add (DeleteDependenceInfo.CreateFromCollection<TEntity> (propName));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddDeleteDependenceFromCollection(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			string propName = PropertyUtil.GetName(propertyRefExpr);
			DeleteItems.Add(DeleteDependenceInfo.CreateFromCollection<TEntity>(propName));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddClearDependence<TDependOn>(Expression<Func<TDependOn, object>> propertyRefExpr)
		{
			ClearItems.Add (ClearDependenceInfo.Create<TDependOn> (propertyRefExpr));
			return this;
		}

		public DeleteInfoHibernate<TEntity> HasSubclasses ()
		{
			IsRootForSubclasses = true;
			return this;
		}

		public DeleteInfoHibernate<TEntity> RequiredCascadeDeletion ()
		{
			IsRequiredCascadeDeletion = true;
			return this;
		}

		#endregion

		#region Функции для внутреннего использования

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity)
		{
			if(depend.PropertyName != null)
			{
				var list = core.UoW.Session.CreateCriteria (ObjectClass)
					.Add (Restrictions.Eq (depend.PropertyName + ".Id", (int)masterEntity.Id)).List ();
				
				return MakeResultList (list);
			}
			else if(depend.CollectionName != null)
			{
				CheckAndLoadEntity(core, masterEntity);
				return MakeResultList (
					masterEntity.Entity.GetPropertyValue (depend.CollectionName) as IList);
			}
			else if(depend.ParentPropertyName != null)
			{
				CheckAndLoadEntity(core, masterEntity);
				var value = (TEntity)masterEntity.Entity.GetPropertyValue(depend.ParentPropertyName);

				return MakeResultList(value == null ? new List<TEntity>() : new List<TEntity> {	value});
			}

			throw new NotImplementedException ();
		}

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, RemoveFromDependenceInfo depend, EntityDTO masterEntity)
		{
			var list = core.UoW.Session.CreateCriteria (ObjectClass)
				.CreateAlias (depend.CollectionName, "childs")
				.Add (Restrictions.Eq (String.Format ( "childs.Id", depend.CollectionName), (int)masterEntity.Id)).List ();

			return MakeResultList (list);
		}

		IList<EntityDTO> IDeleteInfo.GetDependEntities(IDeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity)
		{
			var list = core.UoW.Session.CreateCriteria (ObjectClass)
				.Add (Restrictions.Eq (depend.PropertyName + ".Id", (int)masterEntity.Id)).List ();
			
			return MakeResultList (list);
		}

		private IList<EntityDTO> MakeResultList(IList inputList)
		{
			var resultList = new List<EntityDTO> ();

			foreach(var item in inputList)
			{
				resultList.Add (new EntityDTO{
					Id = (uint)DomainHelper.GetId (item),
					ClassType = ObjectClass,
					Title = DomainHelper.GetObjectTilte (item),
					Entity = item
				});
			}
			return resultList;
		}

		public Operation CreateDeleteOperation(EntityDTO masterEntity, DeleteDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			return new HibernateDeleteOperation {
				ItemId = masterEntity.Id,
				DeletingItems = dependEntities
			};
		}

		public Operation CreateDeleteOperation(EntityDTO entity)
		{
			var list = new List<EntityDTO> ();
			list.Add (entity);

			return new HibernateDeleteOperation {
				DeletingItems = list
			};
		}

		public Operation CreateClearOperation(EntityDTO masterEntity, ClearDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			return new HibernateCleanOperation {
				ItemId = masterEntity.Id,
				ClearingItems = dependEntities,
				EntityType = depend.ObjectClass,
				PropertyName = depend.PropertyName
			};
		}

		public Operation CreateRemoveFromOperation(EntityDTO masterEntity, RemoveFromDependenceInfo depend, IList<EntityDTO> dependEntities)
		{
			return new HibernateRemoveFromCollectionOperation {
				RemoveInClassType = depend.ObjectClass,
				RemoveInItems = dependEntities,
				CollectionName = depend.CollectionName,
				RemoveMethodName = depend.RemoveMethodName,
				RemovingEntity = masterEntity
			};
		}

		EntityDTO IDeleteInfo.GetSelfEntity(IDeleteCore core, uint id)
		{
			var item = core.UoW.GetById<TEntity> ((int)id);
			return new EntityDTO{
				Id = (uint)item.Id,
				ClassType = ObjectClass,
				Title = DomainHelper.GetObjectTilte (item),
				Entity = item
			};
		}

		private bool CheckAndLoadEntity(IDeleteCore core, EntityDTO entity)
		{
			if (entity.Entity != null)
				return true;

			if (entity.ClassType == null)
				throw new InvalidOperationException("EntityDTO без указания класса не может использоваться в связке с NHibernate");

			entity.Entity = core.UoW.GetById(entity.ClassType, (int)entity.Id);
			return entity.Entity != null;
		}

		public Type[] GetSubclasses()
		{
			if (IsRootForSubclasses == false)
				return null;

			return OrmConfig.NhConfig.ClassMappings.Where(x => x.RootClazz.MappedClass == ObjectClass).Select(x => x.MappedClass).ToArray();
		}

		Type IDeleteInfoHibernate.GetRootClass()
		{
			if (!IsSubclass)
				return null;

			var hmap = OrmConfig.NhConfig.GetClassMapping(ObjectClass) as NHibernate.Mapping.Subclass;
			return hmap.RootClazz.MappedClass;
		}

		#endregion
	}
}

