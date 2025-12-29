using System;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;

namespace QS.Project.Journal.Search {
	public class SearchProperty {
		//Класс создается только через фабричные методы, так как в обычном конструкторе нельзя использовать дженерики, без создания еще одного класса.
		#region Фабрика
		public static SearchProperty Create<TEntity>(Expression<Func<TEntity, object>> alias, MatchMode likeMatchMode, Func<string,string> searchPrepareFunc = null) {
			return new SearchProperty(Projections.Property(alias), GetTypeOfProperty(alias.Body), likeMatchMode, searchPrepareFunc);
		}

		public static SearchProperty Create(Expression<Func<object>> alias, MatchMode likeMatchMode, Func<string,string> searchPrepareFunc = null) {
			//Пока IProjection умеем сравнивать только как строки. В идеале научиться вытягивать из них типы.
			if(alias.Body.Type == typeof(IProjection)) 
				return new SearchProperty((IProjection)alias.Compile().Invoke(), typeof(string), likeMatchMode, searchPrepareFunc);
			return new SearchProperty(Projections.Property(alias), GetTypeOfProperty(alias.Body), likeMatchMode, searchPrepareFunc);
		}

		private static Type GetTypeOfProperty(System.Linq.Expressions.Expression body) {
			if(body is UnaryExpression unaryExpression) 
				return unaryExpression.Operand.Type;
			if(body is MemberExpression info)
				return info.Type;
			throw new InvalidOperationException($"Лямбда должна быть {nameof(UnaryExpression)} или {nameof(MemberExpression)}");
		}
		#endregion

		private SearchProperty(IProjection projection, Type typeOfProperty, MatchMode likeMatchMode, Func<string,string> searchPrepareFunc = null) {
			this.projection = projection ?? throw new ArgumentNullException(nameof(projection));
			this.typeOfProperty = typeOfProperty ?? throw new ArgumentNullException(nameof(typeOfProperty));
			this.likeMatchMode = likeMatchMode;
			this.searchPrepareFunc = searchPrepareFunc;
		}
		
		readonly IProjection projection;
		readonly Type typeOfProperty;
		private readonly MatchMode likeMatchMode;
		private readonly Func<string, string> searchPrepareFunc;

		public ICriterion GetCriterion(string searchValue)
		{
			if(searchPrepareFunc != null)
				searchValue = searchPrepareFunc(searchValue);
			
			if (typeOfProperty == typeof(int) || typeOfProperty == typeof(int?)) {
				if(int.TryParse(searchValue, out int intValue)){
					return Restrictions.Eq(projection, intValue);
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
				return Restrictions.Like(Projections.Cast(NHibernateUtil.String, projection), searchValue, likeMatchMode);
			}
			else if (typeOfProperty == typeof(string)){
				return Restrictions.Like(Projections.Cast(NHibernateUtil.String, projection), searchValue, likeMatchMode);
			}
			else if(typeOfProperty == typeof(TimeSpan) || typeOfProperty == typeof(TimeSpan?)) {
				return Restrictions.Like(Projections.Cast(NHibernateUtil.Time, projection), searchValue, likeMatchMode);
			}
			else if(typeOfProperty == typeof(DateTime) || typeOfProperty == typeof(DateTime?)) {
				var dateValue = PrepareDateValue(searchValue);
				if(dateValue != null) 
					return Restrictions.Like(Projections.Cast(NHibernateUtil.DateTime, projection), dateValue, likeMatchMode);
			}
			else if (typeOfProperty.IsEnum){
				return Restrictions.Like(Projections.Cast(NHibernateUtil.String, projection), searchValue, likeMatchMode);
			}
			else 
				throw new NotSupportedException($"Тип {typeOfProperty} не поддерживается");

			return null;
		}

		private string PrepareDateValue(string searchValue) {
			var ruCulture = new System.Globalization.CultureInfo("ru-RU");
			var formats = new[] {
				"dd.MM.yyyy", "dd.MM.yy",    // 26.12.2025, 26.12.25
				"dd/MM/yyyy", "dd/MM/yy",    // 26/12/2025, 26/12/25
				"yyyy-MM-dd",                 // 2025-12-26
				"dd-MM-yyyy", "dd-MM-yy",    // 26-12-2025, 26-12-25
				"d.M.yyyy", "d.M.yy",        // 26.12.2025, 26.12.25 (без ведущих нулей)
				"d/M/yyyy", "d/M/yy",        // 26/12/2025, 26/12/25 (без ведущих нулей)
			};
			
			// Сначала пытаемся распарсить с явными форматами
			if(DateTime.TryParseExact(searchValue, formats, ruCulture, System.Globalization.DateTimeStyles.None, out DateTime dateValue)) {
				return dateValue.ToString("yyyy-MM-dd");
			}
			
			// Если не получилось, пытаемся стандартным способом
			if(DateTime.TryParse(searchValue, ruCulture, System.Globalization.DateTimeStyles.None, out dateValue)) {
				return dateValue.ToString("yyyy-MM-dd");
			}
			
			return null;
		}
	}
}
