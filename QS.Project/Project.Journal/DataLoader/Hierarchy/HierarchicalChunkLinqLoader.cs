using Gamma.Binding.Core.RecursiveTreeConfig;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal.DataLoader.Hierarchy {
	public class HierarchicalChunkLinqLoader<TRoot, TNode> : DynamicQueryLoader<TRoot, TNode>
		where TRoot : class, IDomainObject
		where TNode : class, IHierarchicalNode<TNode> {
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private RecursiveModel<TNode> _recursiveModel;
		private bool? _isRecursive;
		private Func<IUnitOfWork, int?, IQueryable<TNode>> _mainQueryFunc;

		public HierarchicalChunkLinqLoader(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory) {
			HasUnloadedItems = true;
			_unitOfWorkFactory = unitOfWorkFactory;
			LoadedItems = new List<TNode>();
			TreeConfig = new RecursiveConfig<TNode>(x => x.Parent, x => x.Children);
		}

		public IRecursiveConfig TreeConfig { get; }

		public RecursiveModel<TNode> SetRecursiveModel(Func<IUnitOfWork, int?, IQueryable<TNode>> firstLevelQueryFunc) {
			if(_isRecursive == false) {
				throw new InvalidOperationException("Невозможно настроить рекурсивную модель, так как уже была настроена уровневая модель.");
			}
			_mainQueryFunc = firstLevelQueryFunc;
			_isRecursive = true;
			_recursiveModel = new RecursiveModel<TNode>();
			return _recursiveModel;
		}

		public override int GetTotalItemsCount() {
			return _mainQueryFunc(_unitOfWorkFactory.CreateWithoutRoot(), null).Count();
		}

		public virtual void LoadChunk(int? parentId = null) {
			LoadedItems.Clear();
			var items = _mainQueryFunc(_unitOfWorkFactory.CreateWithoutRoot(), parentId).ToList();
			ReorganizeChilds(items);
			LoadedItems.AddRange(items);
		}

		private void ReorganizeChilds(IList<TNode> items, TNode parent = null) {
			foreach(var item in items) {
				if(parent != null) {
					item.Parent = parent;
				}
				if(item.Children.Count > 0) {
					ReorganizeChilds(item.Children, item);
				}
			}
		}

		public override void LoadPage(int? pageSize = null) {
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			LoadChunk();
		}
	}
}
