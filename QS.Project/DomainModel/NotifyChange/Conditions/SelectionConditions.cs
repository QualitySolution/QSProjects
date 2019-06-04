using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.DomainModel.NotifyChange.Conditions
{
	public class SelectionConditions : IConditions
	{
		private readonly List<ICondition> conditions = new List<ICondition>();

		#region Fluent

		public SingleEntityCondition<TEntity> IfEntity<TEntity>()
		{
			var condition = new SingleEntityCondition<TEntity>(this);
			conditions.Add(condition);
			return condition;
		}

		#endregion

		#region Внутренние методы используемые в процессе работы механизма

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			return conditions.Any(x => x.IsSuitable(changeEvent));
		}

		#endregion
	}
}
