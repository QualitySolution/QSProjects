using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using System.Linq;
using System.Collections.Generic;

namespace QS.Project.Journal.EntityLoader
{
	public class DynamicEntityLoader<TEntity, TNode> : IEntityLoader<TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
	{
		private readonly Func<IQueryOver<TEntity>> queryFunc;

		public DynamicEntityLoader(Func<IQueryOver<TEntity>> queryFunc)
		{
			this.queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
			HasUnloadedItems = true;
			TotalItemsCount = null;
			LoadedItems = new List<TNode>();
		}

		#region IEntityLoader implementation

		public bool HasUnloadedItems { get; private set; }

		public int? TotalItemsCount { get; private set; }

		public int LoadedItemsCount => LoadedItems.Count;
		public int ReadedItemsCount { get; set; }

		public List<TNode> LoadedItems { get; private set; }

		public void LoadPage(int? pageSize = null)
		{
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			var workQuery = queryFunc.Invoke().Clone();
			if(pageSize.HasValue) {
				var resultItems = workQuery.Skip(LoadedItemsCount).Take(pageSize.Value).List<TNode>();

				if(resultItems.Count < pageSize) {
					HasUnloadedItems = false;
				}

				LoadedItems.AddRange(resultItems);
			} else {
				LoadedItems = workQuery.List<TNode>().ToList();
				HasUnloadedItems = false;
			}
		}

		public TNode NextUnreadedNode()
		{
			if(ReadedItemsCount >= LoadedItems.Count)
				return null;
			return LoadedItems[ReadedItemsCount];
		}

		public void Reset()
		{
			LoadedItems.Clear();
			ReadedItemsCount = 0;
			HasUnloadedItems = true;
		}

		#endregion IEntityLoader implementation
	}
}
