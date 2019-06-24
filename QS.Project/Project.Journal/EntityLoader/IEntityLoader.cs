using System;
using System.Collections;
using System.Collections.Generic;

namespace QS.Project.Journal.EntityLoader
{
	public interface IEntityLoader<TNode>
		where TNode : JournalEntityNodeBase
	{
		bool HasUnloadedItems { get; }
		List<TNode> LoadItems(int? pageSize = null);
		int LoadedItemsCount { get; }
		int? TotalItemsCount { get; }
		void Refresh();
	}
}
