using System;
using System.Collections;
using System.Collections.Generic;

namespace QS.Project.Journal.EntityLoader
{
	public interface IEntityLoader<TNode>
		where TNode : JournalEntityNodeBase
	{
		bool HasUnloadedItems { get; }
		List<TNode> LoadedItems { get; }
		void LoadPage(int? pageSize = null);
		int LoadedItemsCount { get; }
		int ReadedItemsCount { get; set; }
		TNode NextUnreadedNode();
		int? TotalItemsCount { get; }
		void Reset();
	}
}
