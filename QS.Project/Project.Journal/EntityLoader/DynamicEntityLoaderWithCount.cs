using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.Project.Journal.Search;

namespace QS.Project.Journal.EntityLoader
{
	public class DynamicEntityLoaderWithCount<TEntity, TNode> : IEntityLoader<TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
	{
		private readonly Func<IQueryOver<TEntity>> queryFunc;
		private readonly IQuerySearch querySearch;
		private readonly int defaultPageSize;

		public DynamicEntityLoaderWithCount(Func<IQueryOver<TEntity>> queryFunc, IQuerySearch querySearch, int pageSize = 100)
		{
			this.queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
			this.querySearch = querySearch ?? throw new ArgumentNullException(nameof(querySearch));
			this.defaultPageSize = pageSize;
			HasUnloadedItems = true;
			TotalItemsCount = null;
		}


		#region IEntityLoader implementation

		public bool HasUnloadedItems { get; private set; }

		public int? TotalItemsCount { get; private set; }

		public int LoadedItemsCount { get; private set; }

		public List<TNode> LoadItems(int? pageSize = null)
		{
			ICriterion searchCriterion = querySearch.GetCriterionForSearch();
			var workQuery = queryFunc.Invoke().Clone();
			if(searchCriterion != null) {
				workQuery.Where(searchCriterion);
			}

			var itemsResult = workQuery.Skip(LoadedItemsCount).Take(pageSize ?? defaultPageSize).Future<TNode>();
			if(TotalItemsCount == null) {
				var countResult = workQuery.Clone().ClearOrders().ToRowCountQuery().FutureValue<int>();
				TotalItemsCount = countResult.Value;
			}
			var result = itemsResult.ToList<TNode>();
			var currentLoadItems = result.Count;
			LoadedItemsCount += currentLoadItems;
			if(currentLoadItems < pageSize) {
				HasUnloadedItems = false;
			}
			return result;
		}

		public void Refresh()
		{
			LoadedItemsCount = 0;
			TotalItemsCount = null;
			HasUnloadedItems = true;
		}

		#endregion IEntityLoader implementation

	}
}
