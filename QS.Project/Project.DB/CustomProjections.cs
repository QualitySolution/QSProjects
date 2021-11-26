using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Type;

namespace QS.Project.DB
{
	public static class CustomProjections
	{
		#region Date

		public static IProjection Date(params Expression<Func<object>>[] expressions)
		{
			return Date(expressions.Select(Projections.Property).Cast<IProjection>().ToArray());
		}

		public static IProjection Date(params IProjection[] projections)
		{
			return Projections.SqlFunction("DATE", NHibernateUtil.Date, projections);
		}

		#endregion

		#region Negative

		public static IProjection Negative(IType returnType, Expression<Func<object>> expression)
		{
			return Negative(returnType, Projections.Property(expression));
		}

		public static IProjection Negative(IType returnType, IProjection projection)
		{
			return Projections.SqlFunction(new SQLFunctionTemplate(returnType, "-?1"), returnType, projection);
		}

		#endregion

		#region Cast

		public static IProjection Cast(IType type, Expression<Func<object>> expression)
		{
			if(expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			return Cast(type, Projections.Property(expression));
		}

		public static IProjection Cast(IType type, IProjection projection)
		{
			return new CastProjection(type, projection);
		}

		#endregion

		#region IfNull

		public static IProjection IfNull(params Expression<Func<object>>[] expressions)
		{
			return IfNull(expressions.Select(Projections.Property).Cast<IProjection>().ToArray());
		}

		public static IProjection IfNull(params IProjection[] projections)
		{
			return Projections.SqlFunction("IFNULL", NHibernateUtil.Object, projections);
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

		#region Abs

		public static IProjection Abs(params Expression<Func<object>>[] expressions)
		{
			return Abs(expressions.Select(Projections.Property).Cast<IProjection>().ToArray());
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

		#region Concat_WS

		public static IProjection Concat_WS(string separator = " ", params Expression<Func<object>>[] expressions)
		{
			return Concat_WS(separator, expressions.Select(Projections.Property).Cast<IProjection>().ToArray());
		}

		public static IProjection Concat_WS(string separator = " ", params IProjection[] projections)
		{
			List<IProjection> projectionList = new List<IProjection> { Projections.Constant(separator) };
			projectionList.AddRange(projections);
			return Projections.SqlFunction("CONCAT_WS", NHibernateUtil.String, projectionList.ToArray());
		}

		#endregion

		#region Constant

		public static IProjection Constant<T>(T value)
		{
			return new ConstantProjection(value, NHibernateUtil.GuessType(typeof(T)));
		}

		#endregion

		#region Concat

		public static IProjection Concat(params Expression<Func<object>>[] expressions)
		{
			return Projections.SqlFunction(new StandardSQLFunction("CONCAT"), NHibernateUtil.String,
				expressions.Select(Projections.Property).Cast<IProjection>().ToArray());
		}

		public static IProjection Concat(params IProjection[] projections)
		{
			return Projections.SqlFunction(new StandardSQLFunction("CONCAT"), NHibernateUtil.String, projections);
		}

		#endregion

		#region GetPersonNameWithInitials

		public static IProjection GetPersonNameWithInitials(Expression<Func<object>> lastName, Expression<Func<object>> firstName,
			Expression<Func<object>> patronymic)
		{
			var lastNameProjection = lastName is null ? Constant("") : Projections.Property(lastName);
			var firstNameProjection = firstName is null ? Constant("") : Projections.Property(firstName);
			var patronymicProjection = patronymic is null ? Constant("") : Projections.Property(patronymic);

			return GetPersonNameWithInitials(lastNameProjection, firstNameProjection, patronymicProjection);
		}

		public static IProjection GetPersonNameWithInitials(IProjection lastName, IProjection firstName, IProjection patronymic)
		{
			return Projections.SqlFunction("GET_PERSON_NAME_WITH_INITIALS", NHibernateUtil.String, lastName, firstName, patronymic);
		}

		#endregion
	}

	public enum OrderByDirection
	{
		Asc,
		Desc
	}
}
