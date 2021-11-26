using System;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace QS.Project.DB
{
	public class CustomRestrictions
	{
		public static AbstractCriterion IsNotNull(Expression<Func<object>> expression)
		{
			return new NotNullExpressionExtended(expression);
		}

		public static AbstractCriterion IsNotNull(IProjection projection)
		{
			return new NotNullExpressionExtended(projection);
		}

		public static AbstractCriterion IsNotNull(string propertyName)
		{
			return new NotNullExpressionExtended(propertyName);
		}

		public static AbstractCriterion IsNull(Expression<Func<object>> expression)
		{
			return new NullExpressionExtended(expression);
		}

		public static AbstractCriterion IsNull(IProjection projection)
		{
			return new NullExpressionExtended(projection);
		}

		public static AbstractCriterion IsNull(string propertyName)
		{
			return new NullExpressionExtended(propertyName);
		}
	}

	public class NotNullExpressionExtended : NotNullExpression
	{
		public NotNullExpressionExtended(IProjection projection) : base(projection)
		{ }

		public NotNullExpressionExtended(string propertyName) : base(propertyName)
		{ }

		public NotNullExpressionExtended(Expression<Func<object>> expression) : base(Projections.Property(expression))
		{ }
	}

	public class NullExpressionExtended : NullExpression
	{
		public NullExpressionExtended(IProjection projection) : base(projection)
		{ }

		public NullExpressionExtended(string propertyName) : base(propertyName)
		{ }

		public NullExpressionExtended(Expression<Func<object>> expression) : base(Projections.Property(expression))
		{ }
	}
}
