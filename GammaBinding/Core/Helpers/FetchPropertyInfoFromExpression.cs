using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Gamma.Binding.Core.Helpers
{
	public class FetchPropertyInfoFromExpression : ExpressionVisitor
	{
		List<PropertyInfo> FoundProperies = new List<PropertyInfo>();

		static public PropertyInfo[] Fetch(Expression exp)
		{
			var visitor = new FetchPropertyInfoFromExpression ();
			visitor.Visit (exp);
			return visitor.FoundProperies.ToArray ();
		}

		protected override Expression VisitMemberAccess (MemberExpression m)
		{
			if(m.Member.MemberType == MemberTypes.Property && m.Expression.NodeType == ExpressionType.Parameter)
			{
				var info = m.Member as PropertyInfo;
				if(info != null)
					FoundProperies.Add (info);
				Console.WriteLine (m.Member.Name);
			}
				
			return base.VisitMemberAccess (m);
		}
	}
}

