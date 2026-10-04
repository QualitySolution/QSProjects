using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QS.Utilities.Reflection
{
	/// <summary>
	/// Получение имён и описаний свойств по выражениям вида x => x.Property и работа со свойствами по имени.
	/// </summary>
	public static class PropertyUtil
	{
		public static string GetName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyNameCore(propertyRefExpr.Body);
		}

		public static string GetName<TObject, TProperty>(Expression<Func<TObject, TProperty>> propertyRefExpr)
		{
			return GetPropertyNameCore(propertyRefExpr.Body);
		}

		private static string GetPropertyNameCore(Expression propertyRefExpr)
		{
			if(propertyRefExpr == null)
				throw new ArgumentNullException(nameof(propertyRefExpr), "propertyRefExpr is null.");

			var memberExpr = GetPropertyMemberExpression(propertyRefExpr);
			if(memberExpr != null)
				return memberExpr.Member.Name;

			throw new ArgumentException("No property reference expression was found.", nameof(propertyRefExpr));
		}

		public static PropertyInfo GetPropertyInfo<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyInfo((Expression)propertyRefExpr);
		}

		public static PropertyInfo GetPropertyInfo<TObject, TPropery>(Expression<Func<TObject, TPropery>> propertyRefExpr)
		{
			return GetPropertyInfo((Expression)propertyRefExpr);
		}

		public static PropertyInfo GetPropertyInfo(Expression propertyRefExpr)
		{
			if(propertyRefExpr == null)
				throw new ArgumentNullException(nameof(propertyRefExpr), "propertyRefExpr is null.");

			if(!(propertyRefExpr is LambdaExpression lambda))
				throw new ArgumentException("propertyRefExpr will be lamda function.", nameof(propertyRefExpr));

			return GetPropertyMemberExpression(lambda.Body)?.Member as PropertyInfo;
		}

		/// <summary>
		/// Выражение доступа к свойству, с учётом приведения типа (boxing) вокруг него.
		/// </summary>
		private static MemberExpression GetPropertyMemberExpression(Expression expression)
		{
			var memberExpr = expression as MemberExpression;
			if(memberExpr == null && expression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
				memberExpr = unaryExpr.Operand as MemberExpression;

			return memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property ? memberExpr : null;
		}

		public static object GetPropertyValue(this object subject, string propertyName)
		{
			return subject.GetType().GetProperty(propertyName).GetValue(subject, null);
		}

		public static void SetPropertyValue(this object subject, string propertyName, object value)
		{
			subject.GetType().GetProperty(propertyName).SetValue(subject, value, null);
		}
	}
}
