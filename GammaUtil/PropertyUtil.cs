using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Gamma.Utilities
{
	public static class PropertyUtil
	{
		public static string GetPropertyName<TObject> (this TObject type,
			Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyNameCore (propertyRefExpr.Body);
		}

		public static string GetName<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyNameCore (propertyRefExpr.Body);
		}

		internal static string GetPropertyNameCore (Expression propertyRefExpr)
		{
			if (propertyRefExpr == null)
				throw new ArgumentNullException ("propertyRefExpr", "propertyRefExpr is null.");

			MemberExpression memberExpr = propertyRefExpr as MemberExpression;
			if (memberExpr == null) {
				UnaryExpression unaryExpr = propertyRefExpr as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
				return memberExpr.Member.Name;

			throw new ArgumentException ("No property reference expression was found.",
				"propertyRefExpr");
		}

		public static MemberInfo GetMemberInfo<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			MemberExpression memberExpr = propertyRefExpr.Body as MemberExpression;
			if (memberExpr == null) {
				UnaryExpression unaryExpr = propertyRefExpr.Body as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
				return memberExpr.Member;
			else
				return null;
		}

		public static PropertyInfo GetPropertyInfo<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetMemberInfo (propertyRefExpr) as PropertyInfo;
		}

		public static MemberInfo GetMemberInfo (Expression propertyRefExpr)
		{
			if (propertyRefExpr == null)
				throw new ArgumentNullException ("propertyRefExpr", "propertyRefExpr is null.");

			LambdaExpression lamda = propertyRefExpr as LambdaExpression;

			if (lamda == null)
				throw new ArgumentException ("propertyRefExpr will be lamda function.", "propertyRefExpr");

			MemberExpression memberExpr = lamda.Body as MemberExpression;

			if (memberExpr == null) {
				UnaryExpression unaryExpr = lamda.Body as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
				return memberExpr.Member;
			else
				return null;
		}

		public static PropertyInfo GetPropertyInfo (Expression propertyRefExpr)
		{
			return GetMemberInfo (propertyRefExpr) as PropertyInfo;
		}
	}
}

