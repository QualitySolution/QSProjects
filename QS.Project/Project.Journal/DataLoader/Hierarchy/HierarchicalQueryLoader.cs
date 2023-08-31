using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Impl;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal.DataLoader.Hierarchy {

	public class HierarchicalQueryLoader<TRoot, TNode> : QueryLoader<TRoot, TNode>
		where TRoot : class, IDomainObject
		where TNode : class, IHierarchicalNode<TNode> {

		private Func<IUnitOfWork, IQueryOver<TRoot>> _mainQueryFunc;
		private LevelingModel<TNode> _levelingModel;
		private RecursiveModel<TNode> _recursiveModel;

		/// <summary>
		/// true - рекурсивная модель, false - уровневая, null - не инициализирована
		/// </summary>
		private bool? _isRecursive;

		public HierarchicalQueryLoader(
			IUnitOfWorkFactory unitOfWorkFactory,
			Func<IUnitOfWork, int> itemsCountFunction = null) : base(unitOfWorkFactory, itemsCountFunction) {
			TreeConfig = new RecursiveConfig<TNode>(x => x.Parent, x => x.Children);
		}

		#region Hierarchy

		public IRecursiveConfig TreeConfig { get; }

		/// <summary>
		/// Определяет уровневую модель журнала.
		/// Позволяет добавлять дополнительные уровни для загрузки.
		/// </summary>
		/// <param name="firstLevelQueryFunc">Функция для получения данных для первого уровня. На первый уровень действуют все правила DynamicQueryLoader</param>
		/// <exception cref="InvalidOperationException"></exception>
		public LevelingModel<TNode> SetLevelingModel(Func<IUnitOfWork, IQueryOver<TRoot>> firstLevelQueryFunc) {
			if(_isRecursive == true) {
				throw new InvalidOperationException("Невозможно настроить уровневую модель, так как уже была настроена рекурсивная модель.");
			}
			_mainQueryFunc = firstLevelQueryFunc;
			_isRecursive = false;
			_levelingModel = new LevelingModel<TNode>();
			return _levelingModel;
		}

		/// <summary>
		/// Определяет рекурсивную модель журнала.
		/// Позволяет определить функцию загрузки всех уровней в струтуре.
		/// Необходимо загружать все элементы структуры в одном списке и одним уровнем.
		/// Распределение по уровням происходит автоматически.
		/// </summary>
		/// <param name="firstLevelQueryFunc">Функция для получения данных для первого уровня. На первый уровень действуют все правила DynamicQueryLoader</param>
		/// <exception cref="InvalidOperationException"></exception>
		public RecursiveModel<TNode> SetRecursiveModel(Func<IUnitOfWork, IQueryOver<TRoot>> firstLevelQueryFunc) {
			if(_isRecursive == false) {
				throw new InvalidOperationException("Невозможно настроить рекурсивную модель, так как уже была настроена уровневая модель.");
			}
			_mainQueryFunc = firstLevelQueryFunc;
			_isRecursive = true;
			_recursiveModel = new RecursiveModel<TNode>();
			return _recursiveModel;
		}

		private void LoadChildren(IEnumerable<TNode> parentNodes) {
			if(!_isRecursive.HasValue) {
				throw new InvalidOperationException($"Не настроена иерархическая модель. Необходимо вызвать метод {nameof(SetLevelingModel)} или {nameof(SetRecursiveModel)}");
			}

			foreach(var parent in parentNodes) {
				if(parent.Children == null) {
					parent.Children = new List<TNode>();
				}
			}

			if(_isRecursive.Value) {
				LoadRecursiveChildren(parentNodes);
			}
			else {
				LoadLevelingChildren(parentNodes);
			}
		}

		private void LoadRecursiveChildren(IEnumerable<TNode> parentNodes) {
			var children = _recursiveModel.RecursiveSourceFunc.Invoke(parentNodes);
			if(!parentNodes.Any() || !children.Any()) {
				return;
			}
			var parentDic = parentNodes.ToDictionary(x => x.Id);
			foreach(var child in children) {
				if(!child.ParentId.HasValue) {
					continue;
				}
				var parent = parentDic[child.ParentId.Value];
				if(parent == null) {
					continue;
				}
				if(parent.Children == null) {
					parent.Children = new List<TNode>();
				}
				parent.Children.Add(child);
				child.Parent = parent;
			}
			LoadRecursiveChildren(children);
		}

		private void LoadLevelingChildren(IEnumerable<TNode> parentNodes) {
			foreach(var levelFunc in _levelingModel.LevelingFunctions) {
				var children = levelFunc.Invoke(parentNodes);
				var parentDic = parentNodes.ToDictionary(x => x.Id);
				foreach(var child in children) {
					if(!child.ParentId.HasValue) {
						continue;
					}
					var parent = parentDic[child.ParentId.Value];
					if(parent.Children == null) {
						parent.Children = new List<TNode>();
					}
					parent.Children.Add(child);
					child.Parent = parent;
				}
			}
		}

		#endregion Hierarchy

		#region DynamicQueryLoader override

		public override int GetTotalItemsCount() {
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				if(ItemsCountFunction != null) {
					return ItemsCountFunction(uow);
				}

				var query = _mainQueryFunc.Invoke(uow);
				if(query == null)
					return 0;

				return query.ClearOrders().RowCount();
			}
		}

		public override TNode GetNode(int entityId, IUnitOfWork uow) {
			throw new NotImplementedException();
		}

		public override void LoadPage(int? pageSize = null) {
			//Не подгружаем следующую страницу если из предыдущих данных еще не прочитана целая страница.
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var workQuery = _mainQueryFunc.Invoke(uow);
				if(workQuery == null) {
					HasUnloadedItems = false;
					return;
				}

				if(workQuery.UnderlyingCriteria is CriteriaImpl criteriaImpl && criteriaImpl.Session != uow.Session)
					throw new InvalidOperationException(
						"Метод создания запроса должен использовать переданный ему uow");

				if(pageSize.HasValue) {
					var resultItems = workQuery.Skip(LoadedItemsCount).Take(pageSize.Value).List<TNode>();

					LoadChildren(resultItems);

					HasUnloadedItems = resultItems.Count == pageSize;

					LoadedItems.AddRange(resultItems);
				}
				else {
					LoadedItems = workQuery.List<TNode>().ToList();
					HasUnloadedItems = false;
				}
			}
		}

		#endregion DynamicQueryLoader override
	}
}
