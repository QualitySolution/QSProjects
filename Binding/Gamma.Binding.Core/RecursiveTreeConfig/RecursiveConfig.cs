using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Gamma.Binding.Core.RecursiveTreeConfig
{
	public class RecursiveConfig<TNode> : IRecursiveConfig
	{
		public PropertyInfo ParentProperty { get; }

		public PropertyInfo ChildsCollectionProperty { get; }

		public RecursiveConfig(Expression<Func<TNode, TNode>> parentPropertyExpr, Expression<Func<TNode, IList<TNode>>> childsCollectionPropertyExpr)
		{
			ParentProperty = PropertyUtil.GetPropertyInfo(parentPropertyExpr);
			ChildsCollectionProperty = PropertyUtil.GetPropertyInfo(childsCollectionPropertyExpr);
		}
	}
}
