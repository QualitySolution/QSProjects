using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;

namespace QS.DomainModel.NotifyChange.Conditions
{
	public class SelectionConditions : IConditions
	{
		private readonly List<ICondition> conditions = new List<ICondition>();
		private readonly List<ISession> excludesSesions = new List<ISession>();

		#region Fluent

		/// <summary>
		/// Уведомить только если тип измененого объекта <typeparamref name="TEntity"/>
		/// </summary>
		public SingleEntityCondition<TEntity> IfEntity<TEntity>()
			where TEntity: class
		{
			var condition = new SingleEntityCondition<TEntity>(this);
			conditions.Add(condition);
			return condition;
		}

		public SelectionConditions ExcludeUow(params IUnitOfWork[] unitOfWorks)
		{
			foreach(var uow in unitOfWorks) {
				excludesSesions.Add(uow.Session);
			}
			return this;
		}

		#endregion

		#region Внутренние методы используемые в процессе работы механизма

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if(excludesSesions.Contains(changeEvent.Session))
				return false;
			return conditions.Any(x => x.IsSuitable(changeEvent));
		}

		#endregion
	}
}
