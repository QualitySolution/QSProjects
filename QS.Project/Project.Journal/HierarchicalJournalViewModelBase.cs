using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Linq.Expressions;

namespace QS.Project.Journal
{
	public abstract class HierarchicalJournalViewModelBase<TEntity, TNode> : EntitiesJournalViewModelBase<TNode>
		where TNode : JournalEntityNodeBase, IHierarchicalNode<TNode, TNode>
		where TEntity : class, IDomainObject
	{
		protected Expression<Func<TEntity, object>> _parentEntityPropertyExpr;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		protected HierarchicalJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(unitOfWorkFactory, commonServices, navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		protected virtual Func<IUnitOfWork, IQueryOver<TEntity>> ItemsSourceQueryFunction { get; private set; }

		protected IDataLoader _dataLoader;
		public override IDataLoader DataLoader
		{
			get
			{
				if(_dataLoader == null)
				{
					var loader = new HierarchicalDataLoader<TEntity, TNode>(_unitOfWorkFactory, _parentEntityPropertyExpr);
					loader.SetQueryFunc(ItemsSourceQueryFunction);
					_dataLoader = loader;
				}
				return _dataLoader;
			}
			protected set => _dataLoader = value;
		}
	}
}
