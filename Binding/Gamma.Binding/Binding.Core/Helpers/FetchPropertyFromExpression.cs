using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Gamma.Binding.Core.Helpers
{
	public class FetchPropertyFromExpression : ExpressionVisitor
	{
		List<string> FoundProperies = new List<string>();
		//string inputName;

		static public string[] Fetch(Expression exp)
		{
			var visitor = new FetchPropertyFromExpression ();
/*			var lamda = exp as LambdaExpression;
			if(lamda != null)
			{
				visitor.inputName = lamda.Parameters [0].Name;
			}
*/
			visitor.Visit (exp);
			return visitor.FoundProperies.ToArray ();
		}

		protected override Expression VisitMemberAccess (MemberExpression m)
		{
			if(m.Member.MemberType == System.Reflection.MemberTypes.Property && m.Expression.NodeType == ExpressionType.Parameter)
			{
				FoundProperies.Add (m.Member.Name);
				Console.WriteLine (m.Member.Name);
			}
				
			return base.VisitMemberAccess (m);
		}
	}
}

