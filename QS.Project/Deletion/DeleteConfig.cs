using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace QS.Deletion
{
	public static class DeleteConfig
	{
		public static DeleteConfiguration Main { get; } = new DeleteConfiguration();

		public static IEnumerable<IDeleteRule> ClassDeleteRules => Main.ClassDeleteRules;

		public static void AddDeleteInfo (DeleteInfo info)
		{
			Main.AddDeleteInfo(info);
		}

		public static DeleteInfoHibernate<TEntity> ExistingDeleteRule<TEntity> () where TEntity : IDomainObject
		{
			return Main.ExistingDeleteRule<TEntity>();
		}

		public static IDeleteRule GetDeleteRule(Type clazz)
		{
			return Main.GetDeleteRule(clazz);
		}

		public static void AddDeleteDependence<ToClass> (DeleteDependenceInfo deleteDependence)
		{
			Main.AddDeleteDependence<ToClass>(deleteDependence);
		}

		public static void AddClearDependence<ToClass> (ClearDependenceInfo clearDependence)
		{
			Main.AddClearDependence<ToClass>(clearDependence);
		}

		#region FluentConfig

		public static DeleteInfoHibernate<TEntity> AddHibernateDeleteInfo<TEntity> ()
			where TEntity : IDomainObject
		{
			return Main.AddHibernateDeleteInfo<TEntity>();
		}

		#endregion
	}
}