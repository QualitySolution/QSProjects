using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using System.Linq;

namespace Gamma.Binding
{
	public class RecursiveTreeConfig<TNode> : IRecursiveTreeConfig
	{
		public PropertyInfo ParentProperty { get; private set;}
		public PropertyInfo ChildsCollectionProperty { get; private set;}

		public RecursiveTreeConfig(Expression<Func<TNode, TNode>> parentPropertyExpr, Expression<Func<TNode, IList<TNode>>> childsCollectionPropertyExpr)
		{
			ParentProperty = PropertyUtil.GetPropertyInfo(parentPropertyExpr);
			ChildsCollectionProperty = PropertyUtil.GetPropertyInfo(childsCollectionPropertyExpr);
		}

		public IyTreeModel CreateModel(IList list){
			var genericList = list.Cast<TNode>().Where(x => ParentProperty.GetValue(x, null) == null).ToList();

			return new RecursiveTreeModel<TNode>(genericList, this);
		}
	}
}

