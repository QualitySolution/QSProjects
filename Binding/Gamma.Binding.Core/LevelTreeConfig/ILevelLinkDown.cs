using System.Collections.Generic;

namespace Gamma.Binding.Core.LevelTreeConfig
{
	public interface ILevelLinkDown<TNode, TDown>
	{
		IList<TDown> GetChilds(TNode node);
	}
}

