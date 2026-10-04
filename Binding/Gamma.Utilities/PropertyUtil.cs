using System;
using System.Linq.Expressions;
using System.Reflection;
using NewPropertyUtil = QS.Utilities.Reflection.PropertyUtil;

namespace Gamma.Utilities
{
	/// <summary>
	/// Обёртки над <see cref="QS.Utilities.Reflection.PropertyUtil"/> для совместимости со старым кодом.
	/// </summary>
	public static class PropertyUtil
	{
		private const string UseNew = "Use QS.Utilities.Reflection.PropertyUtil instead.";

		[Obsolete(UseNew)]
		public static string GetPropertyName<TObject> (this TObject type,
			Expression<Func<TObject, object>> propertyRefExpr)
		{
			return NewPropertyUtil.GetName (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static string GetName<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			return NewPropertyUtil.GetName (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static string GetName<TObject, TProperty>(Expression<Func<TObject, TProperty>> propertyRefExpr)
		{
			return NewPropertyUtil.GetName (propertyRefExpr);
		}

		[Obsolete(UseNew + " Use GetPropertyInfo instead.")]
		public static MemberInfo GetMemberInfo<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static PropertyInfo GetPropertyInfo<TObject> (Expression<Func<TObject, object>> propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static PropertyInfo GetPropertyInfo<TObject, TPropery>(Expression<Func<TObject, TPropery>> propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}

		[Obsolete(UseNew + " Use GetPropertyInfo instead.")]
		public static MemberInfo GetMemberInfo (Expression propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static PropertyInfo GetPropertyInfo (Expression propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}

		[Obsolete(UseNew)]
		public static object GetPropertyValue(this object subject, string propertyName)
		{
			return NewPropertyUtil.GetPropertyValue (subject, propertyName);
		}

		[Obsolete(UseNew)]
		public static void SetPropertyValue(this object subject, string propertyName, object value)
		{
			NewPropertyUtil.SetPropertyValue (subject, propertyName, value);
		}

		[Obsolete(UseNew)]
		public static PropertyInfo GetPropertyInfo<TObject> (this TObject type,
			Expression<Func<TObject, object>> propertyRefExpr)
		{
			return NewPropertyUtil.GetPropertyInfo (propertyRefExpr);
		}
	}
}
