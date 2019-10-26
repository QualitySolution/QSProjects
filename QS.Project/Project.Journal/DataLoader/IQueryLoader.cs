using System;
using System.Collections;
using System.Collections.Generic;

namespace QS.Project.Journal.DataLoader
{
	public interface IQueryLoader<TNode>
	{
		bool HasUnloadedItems { get; }
		List<TNode> LoadedItems { get; }
		void LoadPage(int? pageSize = null);
		int LoadedItemsCount { get; }
		int GetTotalItemsCount();
		void Reset();
	}

	public interface IEntityQueryLoader
	{
		Type EntityType { get; }
	}

	public interface IPieceReader<TNode>
	{
		int ReadedItemsCount { get; }
		TNode NextUnreadedNode();
		TNode TakeNextUnreadedNode();
		IList<TNode> TakeAllUnreadedNodes();
	}
}
