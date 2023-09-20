﻿using System;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		/// Уведомить только если тип изменения соответствует <paramref name="typeOfChange"/>.
		/// Можно вызывать несколько раз для добавления разных типов изменений.
		/// </summary>
		public SingleEntityCondition<TEntity> AndChangeType(TypeOfChangeEvent typeOfChange)
		{
			typesOfChange.Add(typeOfChange);
			return this;
		}

		List<Func<TEntity, bool>> whereRistrictions = new List<Func<TEntity, bool>>();

		/// <summary>
		/// Уведомить только если условие <paramref name="predicate"/> истинно.
		/// Можно добавлять несколько условий объединенных через "И" вызывая повторно AndWhere()
		/// </summary>
		/// <param name="predicate">Условие отбора</param>
		public SingleEntityCondition<TEntity> AndWhere(Func<TEntity, bool> predicate)
		{
			whereRistrictions.Add(predicate);
			return this;
		}

		/// <summary>
		/// Или следующее условие для другого типа объекта
		/// </summary>
		public SelectionConditions Or => myConditionsGroup;

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

			if(whereRistrictions.Any(matchFunc => matchFunc(changeEvent.GetEntity<TEntity>()) == false))
				return false;

			return true;
		}
	}
}
