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
		private readonly List<ISession> excludedSessions = new List<ISession>();

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


		/// <summary>
		/// Добавляем в исключения указанные Uow. То есть, уведомления сделанные одним из этих uow, не будут прилетать подписчику.
		/// Например мы можем захотеть не получать уведомления на изменения сделанные в своей сессии.
		/// Обратите внимание, метод можно вызвать несколько раз. Каждый раз список исключений будет расширятся.
		/// </summary>
		/// <returns>The uow.</returns>
		/// <param name="unitOfWorks">Список UnitOfWorks</param>
		public SelectionConditions ExcludeUow(params IUnitOfWork[] unitOfWorks)
		{
			foreach(var uow in unitOfWorks) {
				excludedSessions.Add(uow.Session);
			}
			return this;
		}

		#endregion

		#region Внутренние методы используемые в процессе работы механизма

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if(excludedSessions.Contains(changeEvent.Session))
				return false;
			return conditions.Any(x => x.IsSuitable(changeEvent));
		}

		#endregion
	}
}
