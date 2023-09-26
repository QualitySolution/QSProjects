using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Type;
using QS.DomainModel.Entity;

namespace QS.Deletion
{
	public abstract class Operation
	{
		public uint ItemId;
		public List<Operation> ChildBeforeOperations = new List<Operation> ();
		public List<Operation> ChildAfterOperations = new List<Operation> ();

		public virtual int GetOperationsCount()
		{
			return 1 + 
				ChildAfterOperations.Sum(o => o.GetOperationsCount()) + 
				ChildBeforeOperations.Sum(o => o.GetOperationsCount());
		}

		internal abstract void Execute (IDeleteCore core, CancellationToken cancellation);
	}

	interface IHibernateOperation {}

	class SQLDeleteOperation : Operation
	{
		public string TableName;
		public string WhereStatment;

		internal override void Execute (IDeleteCore core, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return;

			ChildBeforeOperations.ForEach (o => o.Execute (core, cancellation));

			if (cancellation.IsCancellationRequested)
				return;

			core.AddExcuteOperation(String.Format("Удаляем из таблицы {0}", TableName));
			core.ExecuteSql(
				String.Format ("DELETE FROM {0} {1}", TableName, WhereStatment),
				ItemId);

			if (cancellation.IsCancellationRequested)
				return;

			ChildAfterOperations.ForEach (o => o.Execute (core, cancellation));
		}
	}

	class SQLCleanOperation : Operation
	{
		public string TableName;
		public string WhereStatment;
		public string CleanField;

		internal override void Execute (IDeleteCore core, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return;

			var sql = $"UPDATE {TableName} SET {CleanField} = NULL " + WhereStatment;
			core.AddExcuteOperation(String.Format("Очищаем ссылки в таблице {0}", TableName));
			core.ExecuteSql(sql, ItemId);
		}
	}

	class HibernateDeleteOperation : Operation, IHibernateOperation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public IList<EntityDTO> DeletingItems { get; set;}

		internal override void Execute (IDeleteCore core, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return;

			ChildBeforeOperations.ForEach (o => o.Execute (core, cancellation));

			if (cancellation.IsCancellationRequested)
				return;

			core.AddExcuteOperation(String.Format("Удаляем {0}", DomainHelper.GetSubjectNames(DeletingItems[0].Entity)?.NominativePlural));
			foreach(var item in DeletingItems)
			{
				if (cancellation.IsCancellationRequested)
					return;

				logger.Debug ($"Удаляем [{item.Id}] {item.Title}...");
				core.UoW.TryDelete (item.Entity);
			}

			if (cancellation.IsCancellationRequested)
				return;

			ChildAfterOperations.ForEach (o => o.Execute (core, cancellation));
		}
	}

	class HibernateCleanOperation : Operation, IHibernateOperation
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		public EntityDTO CleaningEntity { get; set; }
		public Type EntityType { get; set;}
		public IList<EntityDTO> ClearingItems { get; set;}
		public string PropertyName { get; set;}

		internal override void Execute (IDeleteCore core, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return;

			core.AddExcuteOperation(String.Format("Очищаем ссылки в {0}", DomainHelper.GetSubjectNames(EntityType)?.NominativePlural));
			var propertyCache = EntityType.GetProperty (PropertyName);
			foreach(var item in ClearingItems)
			{
				if (cancellation.IsCancellationRequested)
					return;

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

		internal override void Execute (IDeleteCore core, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
				return;

			core.AddExcuteOperation(String.Format("Очищаем коллекции в {0}", DomainHelper.GetSubjectNames(RemoveInClassType)?.NominativePlural));
			var collectionProp = RemoveInClassType.GetProperty (CollectionName);
			var removeMethod = String.IsNullOrEmpty (RemoveMethodName) ? null : RemoveInClassType.GetMethod (RemoveMethodName);
			foreach(var item in RemoveInItems)
			{
				if (cancellation.IsCancellationRequested)
					return;

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

	class UpdateOperation : Operation, IHibernateOperation {
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public Type UpdateInClassType { get; set; }
		public object UpdatingEntity { get; set; }
		public string PropertyName { get; set; }
		public object PropertyValue { get; set; }
		internal override void Execute(IDeleteCore core, CancellationToken cancellation) {
			if(cancellation.IsCancellationRequested)
				return;

			var propertyCache = UpdateInClassType.GetProperty(PropertyName);

			core.AddExcuteOperation(String.Format("Обновляем значение в {0} свойства {1} на {2}", DomainHelper.GetSubjectNames(UpdateInClassType)?.Accusative, PropertyName, PropertyValue));
			propertyCache.SetValue(UpdatingEntity, PropertyValue, null);
			core.UoW.TrySave(UpdatingEntity);
		}
	}
}
