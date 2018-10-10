using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate;
using QSOrmProject;

namespace QS.Helpers
{
	public static class ListRefreshHelper
	{
		/// <summary>
		/// Перезагружает список значениями из БД
		/// </summary>
		/// <param name="entity">Сущность с которой связан загружаемый список</param>
		/// <param name="session">NHibernate сессия</param>
		/// <param name="listSelector">Селектор списка в указанной сущности, который будет перезагружен</param>
		/// <typeparam name="TEntity">Тип сущности</typeparam>
		/// <typeparam name="TEntityListItem">Тип сущностей в списке</typeparam>
		public static void ReloadListFromDB<TEntity, TEntityListItem>(this TEntity entity, ISession session, Expression<Func<TEntity, IList<TEntityListItem>>> listSelector)
			where TEntity : class, IDomainObject
			where TEntityListItem : class, IDomainObject
		{
			IList<TEntityListItem> list = null;
			var memberSelectorExpression = listSelector.Body as MemberExpression;

			if(memberSelectorExpression == null) { 
				throw new ArgumentException("The body must be a member expression");
			}

			var property = memberSelectorExpression.Member as PropertyInfo;
			if(property == null || property.PropertyType != typeof(IList<TEntityListItem>)) {
				return;
			}

			list = property.GetValue(entity) as IList<TEntityListItem>;

			foreach(var item in list) {
				session.Refresh(item);
			}
			var dbValues = session.QueryOver<TEntityListItem>()
			                      .WhereRestrictionOn(x => x.Id).IsIn(list.Select(x => x.Id).ToArray())
			                      .List()
			                      .ToList();
			list.Clear();
			foreach(var item in dbValues) {
				list.Add(item);
			}
			property.SetValue(entity, list);
		}
	}
}
