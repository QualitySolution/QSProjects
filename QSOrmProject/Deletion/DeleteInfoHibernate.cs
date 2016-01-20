using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

		public IList<EntityDTO> GetEntitiesList(DeleteCore core, DeleteDependenceInfo depend, uint forId)
		{
			return GetEntitiesList (core, depend.PropertyName, forId);
		}

		public IList<EntityDTO> GetEntitiesList(DeleteCore core, ClearDependenceInfo depend, uint forId)
		{
			return GetEntitiesList (core, depend.PropertyName, forId);
		}

		private IList<EntityDTO> GetEntitiesList(DeleteCore core, string propertyName, uint forId)
		{
			var list = core.UoW.Session.CreateCriteria (ObjectClass)
				.Add (Restrictions.Eq (propertyName + ".Id", (int)forId)).List ();

			var resultList = new List<EntityDTO> ();

			foreach(var item in list)
			{
				resultList.Add (new EntityDTO{
					Id = (uint)(item as IDomainObject).Id,
					Title = DomainHelper.GetObjectTilte (item),
					Entity = item
				});
			}
			return resultList;
		}

		public Operation CreateDeleteOperation(DeleteDependenceInfo depend, uint forId)
		{
			return new SQLDeleteOperation {
				ItemId = forId,
				//TableName = TableName,
				WhereStatment = depend.WhereStatment
			};
		}

		public Operation CreateDeleteOperation(uint selfId)
		{
			return new SQLDeleteOperation {
				ItemId = selfId,
				//TableName = TableName,
				WhereStatment = "WHERE id = @id"
			};
		}

		public Operation CreateClearOperation(ClearDependenceInfo depend, uint forId)
		{
			return new SQLCleanOperation () {
				ItemId = forId,
				//TableName = TableName,
				CleanFields = depend.ClearFields,
				WhereStatment = depend.WhereStatment
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

