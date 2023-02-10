using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;

namespace QS.Project.DB
{
	public static class CustomProjections
	{
		#region DATE

		public static IProjection Date(params Expression<Func<object>>[] properties)
		{
			return Date(properties.Select(Projections.Property).ToArray<IProjection>());
		}

		public static IProjection Date(params IProjection[] projections)
		{
			return Projections.SqlFunction("DATE", NHibernateUtil.Date, projections);
		}

		#endregion

		#region ABS

		public static IProjection Abs(params Expression<Func<object>>[] properties)
		{
			return Abs(properties.Select(Projections.Property).ToArray<IProjection>());
		}

		public static IProjection Abs(params IProjection[] projections)
		{
			var firstProjection = projections.FirstOrDefault();
			if(firstProjection == null)
			{
				throw new ArgumentException(@"В SQL функцию ABS не было передано ни одного параметра", nameof(projections));
			}

			var returnType = firstProjection.GetTypes(null, null).FirstOrDefault();
			if(returnType == null)
			{
				throw new InvalidOperationException("Не удалось получить возвращаемый тип проекции");
			}

			return Projections.SqlFunction("ABS", returnType, projections);
		}

		#endregion

		#region CONCAT

		public static IProjection Concat(params Expression<Func<object>>[] properties)
		{
			var projectionList = new List<IProjection>();
			foreach(var propertyExpr in properties)
			{
				switch(propertyExpr.Body)
				{
					case MemberExpression _:
						projectionList.Add(Projections.Property(propertyExpr));
						break;
					case ConstantExpression constantExpr:
						projectionList.Add(Projections.Constant(constantExpr.Value));
						break;
					default: throw new ArgumentException($"Invalid property expression {propertyExpr}", nameof(properties));
				}
			}

			return Concat(projectionList.ToArray());
		}

		public static IProjection Concat(params IProjection[] projections)
		{
			return Projections.SqlFunction(new StandardSQLFunction("CONCAT", NHibernateUtil.String), NHibernateUtil.String, projections);
		}

		#endregion

		#region CONCAT_WS

		public static IProjection Concat_WS(string separator = " ", params Expression<Func<object>>[] properties)
		{
			return Concat_WS(separator, properties.Select(Projections.Property).ToArray<IProjection>());
		}

		public static IProjection Concat_WS(string separator = " ", params IProjection[] projections)
		{
			List<IProjection> projectionList = new List<IProjection> { Projections.Constant(separator) };
			projectionList.AddRange(projections);
			return Projections.SqlFunction("CONCAT_WS", NHibernateUtil.String, projectionList.ToArray());
		}

		#endregion

		#region GROUP_CONCAT

		public static IProjection GroupConcat(
			Expression<Func<object>> expression,
			bool useDistinct = false,
			Expression<Func<object>> orderByExpression = null,
			OrderByDirection orderByDirection = OrderByDirection.Asc,
			string separator = ",")
		{
			if(expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			return GroupConcat(
				Projections.Property(expression),
				useDistinct,
				orderByExpression == null ? null : Projections.Property(orderByExpression),
				orderByDirection,
				separator
			);
		}

		public static IProjection GroupConcat(
			IProjection projection,
			bool useDistinct = false,
			IProjection orderByProjection = null,
			OrderByDirection orderByDirection = OrderByDirection.Asc,
			string separator = ",")
		{
			if(projection == null)
			{
				throw new ArgumentNullException(nameof(projection));
			}
			var separatorProjection = Projections.Constant(separator);

			//DISTINCT + ORDER BY
			if(useDistinct && orderByProjection != null)
			{
				switch(orderByDirection)
				{
					case OrderByDirection.Asc:
						return Projections.SqlFunction(
							"GROUP_CONCAT_DISTINCT_ORDER_BY_ASC",
							NHibernateUtil.String, projection, orderByProjection, separatorProjection);
					case OrderByDirection.Desc:
						return Projections.SqlFunction("GROUP_CONCAT_DISTINCT_ORDER_BY_DESC",
							NHibernateUtil.String, projection, orderByProjection, separatorProjection);
					default:
						throw new NotSupportedException($"{nameof(OrderByDirection)}.{orderByDirection} is not supported");
				}
			}
			//DISTINCT
			if(useDistinct)
			{
				return Projections.SqlFunction("GROUP_CONCAT_DISTINCT", NHibernateUtil.String, projection, separatorProjection);
			}
			//ORDER BY
			if(orderByProjection != null)
			{
				switch(orderByDirection)
				{
					case OrderByDirection.Asc:
						return Projections.SqlFunction("GROUP_CONCAT_ORDER_BY_ASC",
							NHibernateUtil.String, projection, orderByProjection, separatorProjection);
					case OrderByDirection.Desc:
						return Projections.SqlFunction("GROUP_CONCAT_ORDER_BY_DESC",
							NHibernateUtil.String, projection, orderByProjection, separatorProjection);
					default:
						throw new NotSupportedException($"{nameof(OrderByDirection)}.{orderByDirection} is not supported");
				}
			}

			return Projections.SqlFunction("GROUP_CONCAT", NHibernateUtil.String, projection, separatorProjection);
		}

		#endregion

		#region String functions
		
		public static IProjection Lower<T>(Expression<Func<T, object>> expressionProperty) {
			return Lower(Projections.Property(expressionProperty));
		}
		
		public static IProjection Lower(Expression<Func<object>> expressionProperty) {
			return Lower(Projections.Property(expressionProperty));
		}
		
		public static IProjection Lower(IProjection projection) {
			return Projections.SqlFunction("LOWER", NHibernateUtil.String, projection);
		}
		
		public static IProjection Upper<T>(Expression<Func<T, object>> expressionProperty) {
			return Upper(Projections.Property(expressionProperty));
		}
		
		public static IProjection Upper(Expression<Func<object>> expressionProperty) {
			return Upper(Projections.Property(expressionProperty));
		}
		
		public static IProjection Upper(IProjection projection) {
			return Projections.SqlFunction("UPPER", NHibernateUtil.String, projection);
		}

		#endregion
	}

	public enum OrderByDirection
	{
		Asc,
		Desc
	}
}
