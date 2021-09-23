using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Impl;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	public class DynamicQueryLoader<TRoot, TNode> : IQueryLoader<TNode>, IPieceReader<TNode>, IEntityQueryLoader
		where TRoot : class, IDomainObject
		where TNode : class
	{
		private readonly Func<IUnitOfWork, bool, IQueryOver<TRoot>> queryFunc;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		/// <param name="queryFunc">Функция получения запроса, имеет параметры: 
		/// uow - для которого создается запрос 
		/// isCounting - указание является ли запрос подсчетом количества строк </param>
		/// <param name="unitOfWorkFactory">Unit of work factory.</param>
		public DynamicQueryLoader(Func<IUnitOfWork, bool, IQueryOver<TRoot>> queryFunc, IUnitOfWorkFactory unitOfWorkFactory)
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
				var query = queryFunc.Invoke(uow, true);
				if(query == null)
					return 0;

				return query.ClearOrders().RowCount();
			}
		}

		public void LoadPage(int? pageSize = null)
		{
			//Не подгружаем следующую страницу если из предыдущих данных еще не прочитана целая страница.
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			using (var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				var workQuery = queryFunc.Invoke(uow, false);
				if(workQuery == null) {
					HasUnloadedItems = false;
					return;
				}

				if (workQuery.UnderlyingCriteria is CriteriaImpl criteriaImpl && criteriaImpl.Session != uow.Session)
					throw new InvalidOperationException(
						"Метод создания запроса должен использовать переданный ему uow");
				
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

		public IList<TNode> TakeAllUnreadedNodes()
		{
			var readedCount = ReadedItemsCount;
			ReadedItemsCount = LoadedItems.Count;
			return LoadedItems.Skip(readedCount).ToList();
		}

		public TNode GetNode(int entityId, IUnitOfWork uow)
		{
			var query = (queryFunc.Invoke(uow, false) as IQueryOver<TRoot, TRoot>);
			return query.Where(x => x.Id == entityId)
						.Take(1).List<TNode>().FirstOrDefault();
		}

		#endregion

		#region IEntityQueryLoader

		public Type EntityType => typeof(TRoot);

		#endregion
	}
}
