using System;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search {
	public class SearchProperty {
		//Класс создается только через фабричные методы, так как в обычном конструкторе нельзя получить нельзя использовать дженерики, без создания еще одного класса.
		#region Фабрика
		public static SearchProperty Create<TEntity>(Expression<Func<TEntity, object>> alias) {
			return new SearchProperty(Projections.Property(alias), GetTypeOfProperty(alias.Body));
		}

		public static SearchProperty Create(Expression<Func<object>> alias) {
			//Пока IProjection умеем сравнивать только как строки. В идеале научиться вытягивать из них типы.
			if(alias.Body.Type == typeof(IProjection)) 
				return new SearchProperty((IProjection)alias.Compile().Invoke(), typeof(string));
			return new SearchProperty(Projections.Property(alias), GetTypeOfProperty(alias.Body));
		}

		private static Type GetTypeOfProperty(System.Linq.Expressions.Expression body) {
			if(body is UnaryExpression unaryExpression) 
				return unaryExpression.Operand.Type;
			if(body is MemberExpression info)
				return info.Type;
			throw new InvalidOperationException($"Лямбда должна быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
		}
		#endregion

		private SearchProperty(IProjection projection, Type typeOfProperty) {
			this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
			this.typeOfProperty = typeOfProperty ?? throw new ArgumentNullException(nameof(typeOfProperty));
		}
		
		readonly IProjection projection;
		readonly Type typeOfProperty;
		
		public ICriterion GetCriterion(string searchValue)
		{
			if (typeOfProperty == typeof(int) || typeOfProperty == typeof(int?)) {
				if(int.TryParse(searchValue, out int intValue)){
					return Restrictions.Eq(projection, intValue);;
				}
			}
			else if(typeOfProperty == typeof(uint) || typeOfProperty == typeof(uint?)) {
				if(uint.TryParse(searchValue, out uint uintValue)) {
					return Restrictions.Eq(projection, uintValue);
				}
			}
			else if (typeOfProperty == typeof(decimal) || typeOfProperty == typeof(decimal?)) {
				if(decimal.TryParse(searchValue, out decimal decimalValue)) {
					return Restrictions.Eq(projection, decimalValue);
				}
			}
			else if(typeOfProperty == typeof(Guid) || typeOfProperty == typeof(Guid?)) {
				return Restrictions.Like(Projections.Cast(NHibernateUtil.String, projection), searchValue, MatchMode.Anywhere);
			}
			else if (typeOfProperty == typeof(string)){
				return Restrictions.Like(Projections.Cast(NHibernateUtil.String, projection), searchValue, MatchMode.Anywhere);
			}
			else 
				throw new NotSupportedException($"Тип {typeOfProperty} не поддерживается");

			return null;
		}
	}
}
