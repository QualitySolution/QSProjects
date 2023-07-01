using Gamma.Binding.Core.RecursiveTreeConfig;
using NHibernate;
using NHibernate.Impl;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal.DataLoader.Hierarchy {

	public class HierarchicalQueryLoader<TRoot, TNode> : DynamicQueryLoader<TRoot, TNode>
		where TRoot : class, IDomainObject
		where TNode : class, IHierarchicalNode<TNode> {

		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		private Func<IUnitOfWork, IQueryOver<TRoot>> mainQueryFunc;
		private LevelingModel<TNode> levelingModel;
		private RecursiveModel<TNode> recursiveModel;

		/// <summary>
		/// true - рекурсивная модель, false - уровневая, null - не инициализирована
		/// </summary>
		private bool? isRecursive;

		public HierarchicalQueryLoader(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory) {
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			HasUnloadedItems = true;
			LoadedItems = new List<TNode>();
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
			if(isRecursive == true) {
				throw new InvalidOperationException("Невозможно настроить уровневую модель, так как уже была настроена рекурсивная модель.");
			}
			mainQueryFunc = firstLevelQueryFunc;
			isRecursive = false;
			levelingModel = new LevelingModel<TNode>();
			return levelingModel;
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
			if(isRecursive == false) {
				throw new InvalidOperationException("Невозможно настроить рекурсивную модель, так как уже была настроена уровневая модель.");
			}
			mainQueryFunc = firstLevelQueryFunc;
			isRecursive = true;
			recursiveModel = new RecursiveModel<TNode>();
			return recursiveModel;
		}

		private void LoadChildren(IEnumerable<TNode> parentNodes) {
			if(!isRecursive.HasValue) {
				throw new InvalidOperationException($"Не настроена иерархическая модель. Необходимо вызвать метод {nameof(SetLevelingModel)} или {nameof(SetRecursiveModel)}");
			}

			foreach(var parent in parentNodes) {
				if(parent.Children == null) {
					parent.Children = new List<TNode>();
				}
			}

			if(isRecursive.Value) {
				LoadRecursiveChildren(parentNodes);
			}
			else {
				LoadLevelingChildren(parentNodes);
			}
		}

		private void LoadRecursiveChildren(IEnumerable<TNode> parentNodes) {
			var children = recursiveModel.RecursiveSourceFunc.Invoke(parentNodes);
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
			foreach(var levelFunc in levelingModel.LevelingFunctions) {
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
			using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				var query = mainQueryFunc.Invoke(uow);
				if(query == null)
					return 0;

				return query.ClearOrders().RowCount();
			}
		}

		public override void LoadPage(int? pageSize = null) {
			//Не подгружаем следующую страницу если из предыдущих данных еще не прочитана целая страница.
			if(pageSize.HasValue && (LoadedItemsCount - ReadedItemsCount) >= pageSize)
				return;

			using(var uow = unitOfWorkFactory.CreateWithoutRoot()) {
				var workQuery = mainQueryFunc.Invoke(uow);
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
