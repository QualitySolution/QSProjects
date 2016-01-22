using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Utilities;
using NHibernate.Criterion;

namespace QSOrmProject.Deletion
{
	public class DeleteInfoHibernate<TEntity> : IDeleteInfoHibernate
		where TEntity : IDomainObject
	{
		public Type ObjectClass {
			get {
				return typeof(TEntity);
			}
		}
			
		public string ObjectsName {
			get {
				var att = ObjectClass.GetCustomAttributes (typeof(OrmSubjectAttribute), true);
				if (att.Length > 0)
					return (att [0] as OrmSubjectAttribute).AllNames.NominativePlural;
				else
					return null;
			}
		}

		public List<DeleteDependenceInfo> DeleteItems { get; set;}
		public List<ClearDependenceInfo> ClearItems { get; set;}

		public DeleteInfoHibernate()
		{
			DeleteItems = new List<DeleteDependenceInfo>();
			ClearItems = new List<ClearDependenceInfo>();
		}

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

		public DeleteInfoHibernate<TEntity> AddDeleteDependenceFromBag(Expression<Func<TEntity, object>> propertyRefExpr)
		{
			DeleteItems.Add (DeleteDependenceInfo.CreateFromBag<TEntity> (propertyRefExpr));
			return this;
		}

		public DeleteInfoHibernate<TEntity> AddClearDependence<TDependOn>(Expression<Func<TDependOn, object>> propertyRefExpr)
		{
			ClearItems.Add (ClearDependenceInfo.Create<TDependOn> (propertyRefExpr));
			return this;
		}

		public IList<EntityDTO> GetDependEntities(DeleteCore core, DeleteDependenceInfo depend, EntityDTO masterEntity)
		{
			if(depend.PropertyName != null)
			{
				var list = core.UoW.Session.CreateCriteria (ObjectClass)
					.Add (Restrictions.Eq (depend.PropertyName + ".Id", (int)masterEntity.Id)).List ();
				
				return MakeResultList (list);
			}
			else if(depend.CollectionName != null)
			{
				return MakeResultList (
					masterEntity.Entity.GetPropertyValue (depend.CollectionName) as IList);
			}

			throw new NotImplementedException ();
		}

		public IList<EntityDTO> GetDependEntities(DeleteCore core, ClearDependenceInfo depend, EntityDTO masterEntity)
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
					Id = (uint)(item as IDomainObject).Id,
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

		public EntityDTO GetSelfEntity(DeleteCore core, uint id)
		{
			var item = core.UoW.GetById<TEntity> ((int)id);
			return new EntityDTO{
				Id = (uint)item.Id,
				Title = DomainHelper.GetObjectTilte (item),
				Entity = item
			};
		}

	}
}

