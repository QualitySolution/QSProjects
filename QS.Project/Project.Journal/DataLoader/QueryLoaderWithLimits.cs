using System;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	public class QueryLoaderWithLimits<TRoot, TNode> : QueryLoader<TRoot, TNode>
		where TRoot : class, IDomainObject
		where TNode : class
	{
		private readonly Func<IUnitOfWork, int?, int?, bool, IQueryOver<TRoot>> _queryFunc;

		/// <param name="queryFunc">Функция получения запроса, имеет параметры: 
		/// uow - для которого создается запрос
		/// int? skipCount количество элементов которое пропускаем
		/// int? takeCount количество элементов к выборке
		/// isCounting - указание является ли запрос подсчетом количества строк </param>
		/// <param name="unitOfWorkFactory">Unit of work factory.</param>
		/// <param name="itemsCountFunction">Кастомная функция подсчёта кол-ва элементов</param>
		public QueryLoaderWithLimits(
			Func<IUnitOfWork, int?, int?, bool, IQueryOver<TRoot>> queryFunc,
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, int> itemsCountFunction = null) : base(unitOfWorkFactory, itemsCountFunction)
		{
			_queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
		}

		#region IQueryLoader implementation

		public override int GetTotalItemsCount()
		{
			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				if(ItemsCountFunction != null)
				{
					return ItemsCountFunction.Invoke(uow);
				}

				var query = _queryFunc.Invoke(uow, null, null, true);
				if(query == null)
					return 0;

				return query.ClearOrders().RowCount();
			}
		}

		public override void LoadPage(int? pageSize = null)
		{
			//Не подгружаем следующую страницу если из предыдущих данных еще не прочитана целая страница.
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			using (var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var workQuery = _queryFunc.Invoke(uow, LoadedItemsCount, pageSize, false);
				if(workQuery == null) {
					HasUnloadedItems = false;
					return;
				}

				CheckUowSession(workQuery, uow);
				var resultItems = workQuery.List<TNode>();
			
				if (pageSize.HasValue) {
					HasUnloadedItems = resultItems.Count == pageSize;
					LoadedItems.AddRange(resultItems);
				}
				else {
					LoadedItems = resultItems.ToList();
					HasUnloadedItems = false;
				}
			}
		}

		#endregion IEntityLoader implementation

		#region PieceReader

		public override TNode GetNode(int entityId, IUnitOfWork uow)
		{
			var query = _queryFunc.Invoke(uow, null, null, false) as IQueryOver<TRoot, TRoot>;
			return query.Where(x => x.Id == entityId)
						.Take(1).List<TNode>().FirstOrDefault();
		}

		#endregion
	}
}
