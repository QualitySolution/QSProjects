using System;
using System.ComponentModel;
using NHibernate;
using QS.DomainModel.Entity;
using System.Linq;
using System.Collections.Generic;

namespace QS.Project.Journal.EntityLoader
{
	public class AllEntityLoader<TEntity, TNode> : IEntityLoader<TNode>
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		where TNode : JournalEntityNodeBase
	{
		private readonly Func<IQueryOver<TEntity>> queryFunc;

		public AllEntityLoader(Func<IQueryOver<TEntity>> queryFunc)
		{
			this.queryFunc = queryFunc ?? throw new ArgumentNullException(nameof(queryFunc));
			TotalItemsCount = null;
		}

		#region IEntityLoader implementation

		public bool HasUnloadedItems => true;

		public int? TotalItemsCount { get; private set; }

		public int LoadedItemsCount { get; private set; }

		public TNode GetNode(int entityId)
		{
			var query = queryFunc.Invoke().Clone();
			return query.Where(x => x.Id == entityId).Take(1).List<TNode>().FirstOrDefault();
		}

		public List<TNode> LoadItems(int? pageSize = null)
		{
			var items = queryFunc().List<TNode>().ToList();
			TotalItemsCount = items.Count;
			LoadedItemsCount = items.Count;
			return items;
		}

		public void Refresh()
		{
			LoadedItemsCount = 0;
			TotalItemsCount = null;
		}

		#endregion IEntityLoader implementation
	}
}
