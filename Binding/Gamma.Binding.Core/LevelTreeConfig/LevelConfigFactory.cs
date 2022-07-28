using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gamma.Binding.Core.LevelTreeConfig
{
	public static class LevelConfigFactory{
		public static LevelConfig<TNextNode, TypeNotNeed, TNextChildNode> FirstLevel<TNextNode, TNextChildNode>(Expression<Func<TNextNode, IList<TNextChildNode>>> childsCollectionPropertyExpr)
			where TNextChildNode: class
			where TNextNode: class
		{
			return new LevelConfig<TNextNode, TypeNotNeed, TNextChildNode>(null, null, childsCollectionPropertyExpr);
		}
	}
}

