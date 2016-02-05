using System;
using System.Collections;
using System.Collections.Generic;
using QSProjectsLib;

namespace QSOrmProject.Deletion
{
	public abstract class Operation
	{
		public uint ItemId;
		public List<Operation> ChildBeforeOperations = new List<Operation> ();
		public List<Operation> ChildAfterOperations = new List<Operation> ();

		public abstract void Execute (DeleteCore core);
	}

	interface IHibernateOperation {}

	class SQLDeleteOperation : Operation
	{
		public string TableName;
		public string WhereStatment;

		public override void Execute (DeleteCore core)
		{
			ChildBeforeOperations.ForEach (o => o.Execute (core));

			core.ExecuteSql(
				String.Format ("DELETE FROM {0} {1}", TableName, WhereStatment),
				ItemId);

			ChildAfterOperations.ForEach (o => o.Execute (core));
		}
	}

	class SQLCleanOperation : Operation
	{
		public string TableName;
		public string WhereStatment;
		public string CleanField;

		public override void Execute (DeleteCore core)
		{
			var sql = new DBWorks.SQLHelper ("UPDATE {0} SET ", TableName);
			sql.Add ("{0} = NULL ", CleanField);
			sql.Add (WhereStatment);

			core.ExecuteSql(sql.Text, ItemId);
		}
	}

	class HibernateDeleteOperation : Operation, IHibernateOperation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IList<EntityDTO> DeletingItems { get; set;}

		public override void Execute (DeleteCore core)
		{
			ChildBeforeOperations.ForEach (o => o.Execute (core));

			foreach(var item in DeletingItems)
			{
				logger.Debug ("Удаляем {0}...", item.Title);
				core.UoW.TryDelete (item.Entity);
			}

			ChildAfterOperations.ForEach (o => o.Execute (core));
		}
	}

	class HibernateCleanOperation : Operation, IHibernateOperation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type EntityType { get; set;}
		public IList<EntityDTO> ClearingItems { get; set;}
		public string PropertyName { get; set;}

		public override void Execute (DeleteCore core)
		{
			var propertyCache = EntityType.GetProperty (PropertyName);
			foreach(var item in ClearingItems)
			{
				logger.Debug ("Очищаем свойство {0} в {1}...", PropertyName, item.Title);
				propertyCache.SetValue (item.Entity, null, null);
				core.UoW.TrySave (item.Entity);
			}
		}
	}

	class HibernateRemoveFromCollectionOperation : Operation, IHibernateOperation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public Type RemoveInClassType { get; set;}
		public EntityDTO RemovingEntity { get; set;}
		public IList<EntityDTO> RemoveInItems { get; set;}
		public string CollectionName { get; set;}
		public string RemoveMethodName { get; set;}

		public override void Execute (DeleteCore core)
		{
			var collectionProp = RemoveInClassType.GetProperty (CollectionName);
			var removeMethod = String.IsNullOrEmpty (RemoveMethodName) ? null : RemoveInClassType.GetMethod (RemoveMethodName);
			foreach(var item in RemoveInItems)
			{
				logger.Debug ("Удаляем {2} из коллекции {0} в {1}...", CollectionName, item.Title, RemovingEntity.Title);
				if(removeMethod != null)
				{
					if (item.Entity is IBusinessObject)
						(item.Entity as IBusinessObject).UoW = core.UoW;
					removeMethod.Invoke (item.Entity, new object[] {RemovingEntity.Entity});
				} 
				else 
				{
					var collection = (IList)collectionProp.GetValue (item.Entity, null);
					collection.Remove (RemovingEntity.Entity);
				}
				core.UoW.TrySave (item.Entity);
			}
		}
	}

}

