using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal.DataLoader {
	public class SqlQueryLoader<TNode> : IQueryLoader<TNode>, IPieceReader<TNode>
		where TNode : class
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly Func<IUnitOfWork, int?, int?, IList<TNode>> limitedItemsFunction;
		private readonly Func<IUnitOfWork, int> itemsCountFunction;

		public SqlQueryLoader(
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, int?, int?, IList<TNode>> limitedItemsFunction,
			Func<IUnitOfWork, int> itemsCountFunction
		) {
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.limitedItemsFunction = limitedItemsFunction ?? throw new ArgumentNullException(nameof(limitedItemsFunction));
			this.itemsCountFunction = itemsCountFunction ?? throw new ArgumentNullException(nameof(itemsCountFunction));
		}
		public bool HasUnloadedItems { get; protected set; }

		public List<TNode> LoadedItems { get; protected set; } = new List<TNode>();
		public int LoadedItemsCount => LoadedItems.Count;
		public virtual int? TotalItemsCount { get; protected set; }

		public TNode GetNode(int entityId, IUnitOfWork uow) {
			throw new NotSupportedException("В SqlQueryLoader метод GetNode не поддерживается.");
		}

		public int GetTotalItemsCount() {
			using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				return itemsCountFunction.Invoke(uow);
			}
		}

		public void LoadPage(int? pageSize = null) {
			using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				if(pageSize.HasValue) {
					var resultItems = limitedItemsFunction.Invoke(uow, LoadedItemsCount, pageSize.Value);
					HasUnloadedItems = resultItems.Count == pageSize;
					LoadedItems.AddRange(resultItems);
				}
				else {
					LoadedItems = limitedItemsFunction.Invoke(uow, null, null).ToList();
					HasUnloadedItems = false;
				}
			}
		}

		public void Reset() {
			LoadedItems.Clear();
			ReadedItemsCount = 0;
			HasUnloadedItems = true;
		}

		#region PieceReader

		public virtual int ReadedItemsCount { get; protected set; }

		public virtual TNode NextUnreadedNode() {
			if(ReadedItemsCount >= LoadedItems.Count)
				return null;
			return LoadedItems[ReadedItemsCount];
		}

		public virtual TNode TakeNextUnreadedNode() {
			var item = NextUnreadedNode();
			ReadedItemsCount++;
			return item;
		}

		public virtual IList<TNode> TakeAllUnreadedNodes() {
			var readedCount = ReadedItemsCount;
			ReadedItemsCount = LoadedItems.Count;
			return LoadedItems.Skip(readedCount).ToList();
		}

		#endregion
	}
}
