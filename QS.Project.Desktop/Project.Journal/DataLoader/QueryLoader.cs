using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Impl;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace QS.Project.Journal.DataLoader {
	public abstract class QueryLoader<TRoot, TNode> : IQueryLoader<TNode>, IPieceReader<TNode>, IEntityQueryLoader
		where TRoot : class, IDomainObject
		where TNode : class {
		protected readonly IUnitOfWorkFactory UnitOfWorkFactory;
		protected readonly Func<IUnitOfWork, int> ItemsCountFunction;

		protected QueryLoader(IUnitOfWorkFactory unitOfWorkFactory, Func<IUnitOfWork, int> itemsCountFunction = null) {
			UnitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			ItemsCountFunction = itemsCountFunction;
			HasUnloadedItems = true;
			LoadedItems = new List<TNode>();
		}

		#region IQueryLoader implementation

		public virtual bool HasUnloadedItems { get; protected set; }

		public virtual int? TotalItemsCount { get; protected set; }

		public virtual int LoadedItemsCount => LoadedItems.Count;

		public virtual List<TNode> LoadedItems { get; protected set; }

		public abstract int GetTotalItemsCount();

		public abstract void LoadPage(int? pageSize = null);

		public virtual void Reset()
		{
			LoadedItems.Clear();
			ReadedItemsCount = 0;
			HasUnloadedItems = true;
		}

		#endregion IEntityLoader implementation

		#region PieceReader

		public virtual int ReadedItemsCount { get; protected set; }

		public virtual TNode NextUnreadedNode()
		{
			if (ReadedItemsCount >= LoadedItems.Count)
				return null;
			return LoadedItems[ReadedItemsCount];
		}

		public virtual TNode TakeNextUnreadedNode()
		{
			var item = NextUnreadedNode();
			ReadedItemsCount++;
			return item;
		}

		public virtual IList<TNode> TakeAllUnreadedNodes()
		{
			var readedCount = ReadedItemsCount;
			ReadedItemsCount = LoadedItems.Count;
			return LoadedItems.Skip(readedCount).ToList();
		}

		public abstract TNode GetNode(int entityId, IUnitOfWork uow);

		#endregion

		#region IEntityQueryLoader

		public virtual Type EntityType => typeof(TRoot);

		#endregion
		
		protected static void CheckUowSession(IQueryOver<TRoot> workQuery, IUnitOfWork uow)
		{
			if(workQuery.UnderlyingCriteria is CriteriaImpl criteriaImpl && criteriaImpl.Session != uow.Session)
				throw new InvalidOperationException(
					"Метод создания запроса должен использовать переданный ему uow");
		}
	}
}
