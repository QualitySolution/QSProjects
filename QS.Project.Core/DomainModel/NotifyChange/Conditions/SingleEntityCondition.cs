using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gamma.Utilities;

namespace QS.DomainModel.NotifyChange.Conditions
{
	public class SingleEntityCondition<TEntity> : ICondition
		where TEntity: class
	{
		readonly SelectionConditions myConditionsGroup;

		public SingleEntityCondition(SelectionConditions myConditionsGroup)
		{
			this.myConditionsGroup = myConditionsGroup;
		}

		#region Fluent Conditions

		List<TypeOfChangeEvent> typesOfChange = new List<TypeOfChangeEvent>();
		private List<string> changeProperties = new List<string>();

		/// <summary>
		/// Уведомить только если тип изменения соответствует <paramref name="typeOfChange"/>.
		/// Можно вызывать несколько раз для добавления разных типов изменений.
		/// </summary>
		public SingleEntityCondition<TEntity> AndChangeType(TypeOfChangeEvent typeOfChange)
		{
			typesOfChange.Add(typeOfChange);
			return this;
		}

		readonly List<Func<TEntity, bool>> whereRestrictions = new List<Func<TEntity, bool>>();

		/// <summary>
		/// Уведомить только если условие <paramref name="predicate"/> истинно.
		/// Можно добавлять несколько условий объединенных через "И" вызывая повторно AndWhere()
		/// </summary>
		/// <param name="predicate">Условие отбора</param>
		public SingleEntityCondition<TEntity> AndWhere(Func<TEntity, bool> predicate)
		{
			whereRestrictions.Add(predicate);
			return this;
		}

		/// <summary>
		/// Или следующее условие для другого типа объекта
		/// </summary>
		public SelectionConditions Or => myConditionsGroup;

		/// <summary>
		/// И если были изменения любого из свойств объекта.
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		public SingleEntityCondition<TEntity> AndDiffAnyOfProperties(params Expression<Func<TEntity, object>>[] properties) {
			foreach(var prop in properties)
				changeProperties.Add(PropertyUtil.GetName(prop));
			return this;
		}
		#endregion

		/// <summary>
		/// Для внутреннего использования. Возвращает результат проверки.
		/// </summary>
		public bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if (changeEvent.EntityClass != typeof(TEntity))
				return false;

			if (typesOfChange.Any() && !typesOfChange.Contains(changeEvent.EventType))
				return false;

			if(whereRestrictions.Any(matchFunc => matchFunc(changeEvent.GetEntity<TEntity>()) == false))
				return false;

			if(changeProperties.Any() && changeProperties.All(p => !changeEvent.IsDiff(p)))
				return false;

			return true;
		}
	}
}
