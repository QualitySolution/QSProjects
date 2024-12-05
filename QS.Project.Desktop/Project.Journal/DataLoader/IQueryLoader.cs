using System;
using System.Collections;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	public interface IQueryLoader<TNode>
	{
		bool HasUnloadedItems { get; }
		List<TNode> LoadedItems { get; }
		void LoadPage(int? pageSize = null);
		int LoadedItemsCount { get; }
		int GetTotalItemsCount();
		TNode GetNode(int entityId, IUnitOfWork uow);
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
