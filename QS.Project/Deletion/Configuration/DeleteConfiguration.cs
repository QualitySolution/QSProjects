using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace QS.Deletion.Configuration
{
	public class DeleteConfiguration
	{
		internal List<IDeleteInfo> ClassInfos { get; } = new List<IDeleteInfo> ();

		public IEnumerable<IDeleteRule> ClassDeleteRules => ClassInfos;

		public void AddDeleteInfo (DeleteInfo info)
		{
			if (info.ObjectClass != null && ClassInfos.Exists (i => i.ObjectClass == info.ObjectClass))
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} уже существует.", info.ObjectClass));

			if (ClassInfos.OfType<DeleteInfo>().Any (i => i.TableName == info.TableName && i.ObjectClass == info.ObjectClass))
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} и таблицы {1}, уже существует.", info.ObjectClass, info.TableName));

			ClassInfos.Add (info);
		}

		public DeleteInfoHibernate<TEntity> ExistingDeleteRule<TEntity> () where TEntity : IDomainObject
		{
			return ClassInfos.OfType<DeleteInfoHibernate<TEntity>>().First();
		}

		public IDeleteRule GetDeleteRule(Type clazz)
		{
			return ClassInfos.FirstOrDefault(i => i.ObjectClass == clazz);
		}

		#region Внутренне использование

		internal IDeleteInfo GetDeleteInfo (Type clazz)
		{
			return ClassInfos.FirstOrDefault (i => i.ObjectClass == clazz);
		}

		internal IDeleteInfo GetDeleteInfo<T>()
		{
			return ClassInfos.FirstOrDefault(i => i.ObjectClass == typeof(T));
		}

		internal IDeleteInfo GetDeleteInfo(RemoveFromDependenceInfo removeFromDependence)
		{
			return ClassInfos.Find(i => i.ObjectClass == removeFromDependence.ObjectClass);
		}

		internal IDeleteInfo GetDeleteInfo(ClearDependenceInfo clearDependence)
		{ 
			if (clearDependence.ObjectClass != null)
				return ClassInfos.Find(i => i.ObjectClass == clearDependence.ObjectClass);
			else
				return ClassInfos.OfType<DeleteInfo>().First(i => i.TableName == clearDependence.TableName);
		}

		internal IDeleteInfo GetDeleteInfo(DeleteDependenceInfo deleteDependence)
		{
			if (deleteDependence.ObjectClass != null)
				return ClassInfos.Find(i => i.ObjectClass == deleteDependence.ObjectClass);
			else
				return ClassInfos.OfType<DeleteInfo>().First(i => i.TableName == deleteDependence.TableName);
		}


		#endregion

		public void AddDeleteDependence<ToClass> (DeleteDependenceInfo deleteDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if (info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.DeleteItems.Add (deleteDependence);
		}

		public void AddClearDependence<ToClass> (ClearDependenceInfo clearDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if (info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.ClearItems.Add (clearDependence);
		}

		#region FluentConfig

		public DeleteInfoHibernate<TEntity> AddHibernateDeleteInfo<TEntity> ()
			where TEntity : IDomainObject
		{
			var info = (DeleteInfoHibernate<TEntity>) ClassInfos.Find (i => i.ObjectClass == typeof(TEntity));
			if (info != null)
				return info;

			info = new DeleteInfoHibernate<TEntity> ();
			ClassInfos.Add (info);
			return info;
		}

		#endregion
	}

	public class DeletedItem
	{
		public uint ItemId;
		public Type ItemClass;
		public string Title;
	}
}
