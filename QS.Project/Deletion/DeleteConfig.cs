using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace QS.Deletion
{
	public static class DeleteConfig
	{
		private static List<IDeleteInfo> classInfos;

		internal static List<IDeleteInfo> ClassInfos {
			get {
				if (classInfos == null) {
					classInfos = new List<IDeleteInfo> ();
				}
				return classInfos;
			}
		}

		public static IEnumerable<IDeleteRule> ClassDeleteRules => ClassInfos;

		public static event EventHandler<AfterDeletionEventArgs> AfterDeletion;



		public static void AddDeleteInfo (DeleteInfo info)
		{
			if (info.ObjectClass != null && ClassInfos.Exists (i => i.ObjectClass == info.ObjectClass))
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} уже существует.", info.ObjectClass));

			if (ClassInfos.OfType<DeleteInfo>().Any (i => i.TableName == info.TableName && i.ObjectClass == info.ObjectClass))
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} и таблицы {1}, уже существует.", info.ObjectClass, info.TableName));

			ClassInfos.Add (info);
		}

		public static DeleteInfoHibernate<TEntity> ExistingDeleteRule<TEntity> () where TEntity : IDomainObject
		{
			return ClassInfos.OfType<DeleteInfoHibernate<TEntity>>().First();
		}

		public static IDeleteRule GetDeleteRule(Type clazz)
		{
			return ClassInfos.FirstOrDefault(i => i.ObjectClass == clazz);
		}

		internal static IDeleteInfo GetDeleteInfo (Type clazz)
		{
			return ClassInfos.FirstOrDefault (i => i.ObjectClass == clazz);
		}

		internal static IDeleteInfo GetDeleteInfo<T>()
		{
			return ClassInfos.FirstOrDefault(i => i.ObjectClass == typeof(T));
		}

		public static void AddDeleteDependence<ToClass> (DeleteDependenceInfo deleteDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if (info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.DeleteItems.Add (deleteDependence);
		}

		public static void AddClearDependence<ToClass> (ClearDependenceInfo clearDependence)
		{
			var info = ClassInfos.Find (i => i.ObjectClass == typeof(ToClass));

			if (info == null)
				throw new InvalidOperationException (String.Format ("Описание удаления для класса {0} не найдено.", typeof(ToClass)));

			info.ClearItems.Add (clearDependence);
		}

		internal static void OnAfterDeletion (System.Data.Common.DbTransaction trans, List<DeletedItem> items)
		{
			if (AfterDeletion != null) {
				AfterDeletion (null, new AfterDeletionEventArgs {
					CurTransaction = trans,
					DeletedItems = items
				});
			}
		}

		#region FluentConfig

		public static DeleteInfoHibernate<TEntity> AddHibernateDeleteInfo<TEntity> ()
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

	public class AfterDeletionEventArgs : EventArgs
	{
		public System.Data.Common.DbTransaction CurTransaction;
		public List<DeletedItem> DeletedItems;
	}

	public class DeletedItem
	{
		public uint ItemId;
		public Type ItemClass;
		public string Title;
	}
}
