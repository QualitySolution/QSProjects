using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	public class DynamicQueryLoader<TRoot, TNode> : IQueryLoader<TNode>, IPieceReader<TNode>, IEntityQueryLoader
		where TRoot : class
		where TNode : class
	{
		private readonly Func<IUnitOfWork, IQueryOver<TRoot>> queryFunc;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public DynamicQueryLoader(Func<IUnitOfWork, IQueryOver<TRoot>> queryFunc, IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			HasUnloadedItems = true;
			LoadedItems = new List<TNode>();
		}

		#region IQueryLoader implementation

		public bool HasUnloadedItems { get; private set; }

		public int? TotalItemsCount { get; private set; }

		public int LoadedItemsCount => LoadedItems.Count;

		public List<TNode> LoadedItems { get; private set; }

		public int GetTotalItemsCount()
		{
			using (var uow = unitOfWorkFactory.CreateWithoutRoot()) {

				return queryFunc.Invoke(uow).ClearOrders().RowCount();
			}
		}

		public void LoadPage(int? pageSize = null)
		{
			//Не подгружаем следующую страницу если из предыдущих данных еще не прочитана целая страница.
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			using (var uow = unitOfWorkFactory.CreateWithoutRoot()) {

				var workQuery = queryFunc.Invoke(uow);
				if (pageSize.HasValue) {
					var resultItems = workQuery.Skip(LoadedItemsCount).Take(pageSize.Value).List<TNode>();

					HasUnloadedItems = resultItems.Count == pageSize;

					LoadedItems.AddRange(resultItems);
				}
				else {
					LoadedItems = workQuery.List<TNode>().ToList();
					HasUnloadedItems = false;
				}
			}
		}

		public void Reset()
		{
			LoadedItems.Clear();
			ReadedItemsCount = 0;
			HasUnloadedItems = true;
		}

		#endregion IEntityLoader implementation

		#region PieceReader

		public int ReadedItemsCount { get; set; }

		public TNode NextUnreadedNode()
		{
			if (ReadedItemsCount >= LoadedItems.Count)
				return null;
			return LoadedItems[ReadedItemsCount];
		}

		public TNode TakeNextUnreadedNode()
		{
			var item = NextUnreadedNode();
			ReadedItemsCount++;
			return item;
		}

		#endregion

		#region IEntityQueryLoader

		public Type EntityType => typeof(TRoot);

		#endregion
	}
}
