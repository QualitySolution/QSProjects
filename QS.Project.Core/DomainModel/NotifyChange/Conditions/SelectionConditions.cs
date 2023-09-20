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
		private readonly List<ISession> onlyForSessions = new List<ISession>();

		#region Fluent

		/// <summary>
		/// Уведомить только если тип измененного объекта <typeparamref name="TEntity"/>
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
		/// <param name="unitOfWorks">Список UnitOfWorks</param>
		public SelectionConditions ExcludeUow(params IUnitOfWork[] unitOfWorks)
		{
			foreach(var uow in unitOfWorks) {
				excludedSessions.Add(uow.Session);
			}
			return this;
		}
		
		/// <summary>
		/// Метод ограничит получение уведомлений только из указанных UOW.
		/// Вы можете захотеть получать уведомления только из своей сессии.
		/// Обратите внимание, вы можете вызывать метод несколько раз, добавляя в список новые uow.
		/// </summary>
		/// <param name="unitOfWorks">Список UnitOfWorks</param>
		public SelectionConditions OnlyForUow(params IUnitOfWork[] unitOfWorks)
		{
			foreach(var uow in unitOfWorks) {
				onlyForSessions.Add(uow.Session);
			}
			return this;
		}

		#endregion

		#region Внутренние методы используемые в процессе работы механизма

		internal bool IsSuitable(EntityChangeEvent changeEvent)
		{
			if(excludedSessions.Contains(changeEvent.Session))
				return false;
			if(onlyForSessions.Any() && !onlyForSessions.Contains(changeEvent.Session))
				return false;
			return !conditions.Any() || conditions.Any(x => x.IsSuitable(changeEvent));
		}

		#endregion
	}
}
