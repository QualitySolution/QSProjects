using System;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader
{
	public class DynamicQueryLoader<TRoot, TNode> : QueryLoader<TRoot, TNode>
		where TRoot : class, IDomainObject
		where TNode : class
	{
		protected Func<IUnitOfWork, bool, IQueryOver<TRoot>> QueryFunc;
		
		/// <param name="queryFunc">Функция получения запроса, имеет параметры: 
		/// uow - для которого создается запрос 
		/// isCounting - указание является ли запрос подсчетом количества строк </param>
		/// <param name="unitOfWorkFactory">Unit of work factory.</param>
		/// <param name="itemsCountFunction">Кастомная функция подсчёта кол-ва элементов</param>
		public DynamicQueryLoader(
			Func<IUnitOfWork, bool, IQueryOver<TRoot>> queryFunc,
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, int> itemsCountFunction = null) : base(unitOfWorkFactory, itemsCountFunction)
		{
			QueryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
		}

		#region IQueryLoader implementation

		public override int GetTotalItemsCount()
		{
			using (var uow = UnitOfWorkFactory.Create()) {
				if(ItemsCountFunction != null)
				{
					return ItemsCountFunction.Invoke(uow);
				}

				var query = QueryFunc.Invoke(uow, true);
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

			using (var uow = UnitOfWorkFactory.Create()) {
				var workQuery = QueryFunc.Invoke(uow, false);
				if(workQuery == null) {
					HasUnloadedItems = false;
					return;
				}

				CheckUowSession(workQuery, uow);
			
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

		#endregion IEntityLoader implementation

		#region PieceReader

		public override TNode GetNode(int entityId, IUnitOfWork uow)
		{
			var query = QueryFunc.Invoke(uow, false) as IQueryOver<TRoot, TRoot>;
			return query.Where(x => x.Id == entityId)
						.Take(1).List<TNode>().FirstOrDefault();
		}

		#endregion
	}
}
