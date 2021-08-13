using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gamma.Utilities
{
	public class PropertyChainFromExpression
	{
		private List<PropertyInfo> properies = new List<PropertyInfo>();

		public static string GetChainName(Expression exp)
		{
			return GetChainName (Get (exp));
		}

		public static string GetChainName(PropertyInfo[] chain)
		{
			return String.Join (".", chain.Select (p => p.Name));
		}

		public static PropertyInfo[] Get(Expression exp)
		{
			var chainParser = new PropertyChainFromExpression ();
			if(exp.NodeType == ExpressionType.Lambda)
				chainParser.Parse (((LambdaExpression)exp).Body);
			else
				chainParser.Parse (exp);
			chainParser.properies.Reverse ();
			return chainParser.properies.ToArray ();
		}

		private void Parse(Expression exp)
		{
			MemberExpression memberExpr = exp as MemberExpression;
			if (memberExpr == null) {
				UnaryExpression unaryExpr = exp as UnaryExpression;
				if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
					memberExpr = unaryExpr.Operand as MemberExpression;
			}

			if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
			{
				properies.Add (memberExpr.Member as PropertyInfo);
				if (memberExpr.Expression.NodeType == ExpressionType.Parameter)
					return;
				if (memberExpr.Expression != null)
					Parse (memberExpr.Expression);
			}
			else
				throw new InvalidCastException();
		}
	}
}

