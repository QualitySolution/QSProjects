using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using QS.Project.Journal.Search;
using NHibernate.Criterion;
using System.Linq;
using System.Collections.Generic;

namespace QS.Project.Journal.EntityLoader
{
	public class DynamicEntityLoader<TEntity, TNode> : IEntityLoader<TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
	{
		private readonly Func<IQueryOver<TEntity>> queryFunc;
		private readonly IQuerySearch querySearch;
		private readonly int defaultPageSize;

		public DynamicEntityLoader(Func<IQueryOver<TEntity>> queryFunc, IQuerySearch querySearch, int defaultPageSize = 100)
		{
			this.queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
			this.querySearch = querySearch ?? throw new ArgumentNullException(nameof(querySearch));
			this.defaultPageSize = defaultPageSize;
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
			var resultItems = workQuery.Skip(LoadedItemsCount).Take(pageSize ?? defaultPageSize).List<TNode>().ToList();
			var currentLoadItems = resultItems.Count;
			LoadedItemsCount += currentLoadItems;
			if(currentLoadItems < pageSize) {
				HasUnloadedItems = false;
			}
			return resultItems;
		}

		public void Refresh()
		{
			LoadedItemsCount = 0;
			HasUnloadedItems = true;
		}

		#endregion IEntityLoader implementation

	}
}
