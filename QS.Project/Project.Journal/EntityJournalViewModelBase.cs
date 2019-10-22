﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Util;
using QS.Deletion;
using QS.DomainModel.Entity;
using QS.Project.Journal.EntityLoader;
using QS.Project.Journal.Search;
using QS.Services;
using QS.Tdi;

namespace QS.Project.Journal
{
	public abstract class EntityJournalViewModelBase<TNode> : JournalViewModelBase
		where TNode : JournalEntityNodeBase
	{
		private const int defaultPageSize = 100;

		private readonly ICommonServices commonServices;

		protected Dictionary<Type, JournalEntityConfig<TNode>> EntityConfigs { get; private set; }

		public override Type NodeType => typeof(TNode);

		private IJournalFilter filter;
		public override IJournalFilter Filter {
			get => filter;
			protected set {
				if(filter != null)
					filter.OnFiltered -= FilterViewModel_OnFiltered;
				filter = value;
				if(filter != null)
					filter.OnFiltered += FilterViewModel_OnFiltered;
			}
		}

		public event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;

		protected EntityJournalViewModelBase(ICommonServices commonServices) : base(commonServices?.InteractiveService)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UseSlider = true;
			PageSize = defaultPageSize;
			DynamicLoadingEnabled = true;
			EntityConfigs = new Dictionary<Type, JournalEntityConfig<TNode>>();
			Search.OnSearch += Search_OnSearch;
			searchHelper = new SearchHelper(Search);
		}

		void Search_OnSearch(object sender, EventArgs e)
		{
			Refresh();
		}

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		internal override void OnItemsSelected(params object[] selectedNodes)
		{
			OnEntitySelectedResult.Invoke(this, new JournalSelectedNodesEventArgs(selectedNodes.Cast<JournalEntityNodeBase>().ToArray()));
			Close(false);
		}

		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity<TEntity>(Func<IQueryOver<TEntity>> queryFunction)
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		{
			if(queryFunction == null) {
				throw new ArgumentNullException(nameof(queryFunction));
			}

			CreateLoader(queryFunction);

			var configurator = new JournalEntityConfigurator<TEntity, TNode>();
			configurator.OnConfigurationFinished += (sender, e) => {
				var config = e.Config;
				if(EntityConfigs.ContainsKey(config.EntityType)) {
					throw new InvalidOperationException($"Конфигурация для сущности ({config.EntityType.Name}) уже была добавлена.");
				}
				EntityConfigs.Add(config.EntityType, config);
			};
			return configurator;
		}

		protected void FinishJournalConfiguration()
		{
			UpdateAllEntityPermissions();
			CreateNodeActions();
			CreatePopupActions();
		}

		public sealed override void Refresh()
		{
			ClearCachedItems();
			TryLoad();
		}

		protected virtual void ClearCachedItems()
		{
			allRowsCount = null;
			loadedCount = 0;
			Items.Clear();
			foreach(var item in entityLoaders.Values) {
				item.Refresh();
			}
		}

		#region Ordering

		private bool isDescOrder = false;
		private Func<TNode, object> orderFunction = x => x.Id;

		protected void SetOrder(Func<TNode, object> orderFunc, bool desc = false)
		{
			orderFunction = orderFunc;
			isDescOrder = desc;
		}

		Dictionary<Func<TNode, object>, bool> orderingDictionary;
		/// <summary>
		/// Сортировка по нескольким полям. Функция сортировки и порядок 
		/// передаются через словарь <see cref="orderingDictionary"/>,
		/// где его ключём является сама функция, а значением - порядок
		/// сортировки. Значение <c>true</c> указывает на сортировку по убыванию. 
		/// </summary>
		/// <param name="orderingDictionary">Словарь с функциями сортировки и
		/// направлением сортировки</param>
		protected void SetOrder(Dictionary<Func<TNode, object>, bool> orderingDictionary) => this.orderingDictionary = orderingDictionary;

		#endregion Ordering

		#region Search

		private readonly SearchHelper searchHelper;

		protected ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			return searchHelper.GetSearchCriterion(aliasPropertiesExpr);
		}

		protected ICriterion GetSearchCriterion<TEntity>(params Expression<Func<TEntity, object>>[] propertiesExpr)
		{
			return searchHelper.GetSearchCriterion(propertiesExpr);
		}

		#endregion Search

		#region Dynamic load

		protected int PageSize { get; set; }

		private int? allRowsCount = null;
		private int loadedCount = 0;

		public override bool TryLoad()
		{
			if(!EntityConfigs.Any()) {
				return false;
			}

			List<TNode> summaryItems = new List<TNode>();


			var loaders = entityLoaders.Values.ToList();
			var entityCountsForLoad = loaders.Count(x => x.HasUnloadedItems);
			if(entityCountsForLoad == 0) {
				return false;
			}

			int currentPageSize = PageSize / entityCountsForLoad;

			foreach(var loader in loaders) {
				var loadedItems = loader.LoadItems(currentPageSize);
				summaryItems.AddRange(loadedItems);
			}

			var orderedItems = OrderItems(summaryItems);

			UpdateItems(orderedItems);

			return loaders.Any(x => x.HasUnloadedItems);
		}

		private List<TNode> OrderItems(List<TNode> items)
		{
			if(orderingDictionary != null && orderingDictionary.Any()) {
				IOrderedEnumerable<TNode> resultItems = null;
				bool isFirstValueInDictionary = true;
				foreach(var item in orderingDictionary) {
					if(isFirstValueInDictionary) {
						if(item.Value)
							resultItems = items.OrderByDescending(item.Key);
						resultItems = items.OrderBy(item.Key);
						isFirstValueInDictionary = false;
					} else {
						if(item.Value)
							resultItems = resultItems.ThenByDescending(item.Key);
						resultItems = resultItems.ThenBy(item.Key);
					}
				}
				return resultItems.ToList();
			}

			if(isDescOrder)
				return items.OrderByDescending(orderFunction).ToList();
			return items.OrderBy(orderFunction).ToList();
		}

		protected override void UpdateItems(IList items)
		{
			if(DynamicLoadingEnabled) {
				foreach(var item in items) {
					Items.Add(item);
				}
			} else {
				Items = items;
			}
			RaiseItemsUpdated();
		}

		#region Entity load configuration

		private Dictionary<Type, IEntityLoader<TNode>> entityLoaders = new Dictionary<Type, IEntityLoader<TNode>>();

		private void CreateLoader<TEntity>(Func<IQueryOver<TEntity>> queryFunc)
			where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		{
			var entityType = typeof(TEntity);
			if(entityLoaders.ContainsKey(entityType)) {
				return;
			}

			IEntityLoader<TNode> loader = null;
			if(DynamicLoadingEnabled) {
				loader = new DynamicEntityLoader<TEntity, TNode>(queryFunc);
			} else {
				loader = new AllEntityLoader<TEntity, TNode>(queryFunc);
			}

			entityLoaders.Add(entityType, loader);
		}


		#endregion Entity load configuration

		#endregion Dynamic load

		#region Permissions

		protected void UpdateAllEntityPermissions()
		{
			foreach(var entityConfig in EntityConfigs) {
				UpdateEntityPermissions(entityConfig.Key);
			}
		}

		protected virtual void UpdateEntityPermissions<TEntity>()
		{
			UpdateEntityPermissions(typeof(TEntity));
		}

		protected virtual void UpdateEntityPermissions(Type entityType)
		{
			IPermissionResult entityPermissionResult = commonServices.PermissionService.ValidateUserPermission(entityType, commonServices.UserService.CurrentUserId);

			if(EntityConfigs.ContainsKey(entityType)) {
				EntityConfigs[entityType].PermissionResult = entityPermissionResult;
			}
		}

		#endregion Permissions

		#region Actions

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateDefaultDeleteAction();
		}

		protected override void CreatePopupActions()
		{
		}

		private void CreateDefaultAddActions()
		{
			if(!EntityConfigs.Any()) {
				return;
			}

			var totalCreateDialogConfigs = EntityConfigs
				.Where(x => x.Value.PermissionResult.CanCreate)
				.Sum(x => x.Value.EntityDocumentConfigurations
							.Select(y => y.GetCreateEntityDlgConfigs().Count())
							.Sum());

			if(EntityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1) {
				var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });
				foreach(var entityConfig in EntityConfigs.Values) {
					foreach(var documentConfig in entityConfig.EntityDocumentConfigurations) {
						foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs()) {
							var childNodeAction = new JournalAction(createDlgConfig.Title,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) => entityConfig.PermissionResult.CanCreate,
								(selected) => {
									TabParent.OpenTab(createDlgConfig.OpenEntityDialogFunction, this);
									if(documentConfig.JournalParameters.HideJournalForCreateDialog) {
										HideJournal(TabParent);
									}
								}
							);
							addParentNodeAction.ChildActionsList.Add(childNodeAction);
						}
					}
				}
				NodeActionsList.Add(addParentNodeAction);
			} else {
				var entityConfig = EntityConfigs.First().Value;
				var addAction = new JournalAction("Добавить",
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) => entityConfig.PermissionResult.CanCreate,
					(selected) => {
						var docConfig = entityConfig.EntityDocumentConfigurations.First();
						ITdiTab tab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction();
						TabParent.OpenTab(() => tab, this);
						if(docConfig.JournalParameters.HideJournalForCreateDialog) {
							HideJournal(TabParent);
						}
					});
				NodeActionsList.Add(addAction);
			};
		}

		private void CreateDefaultEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) => {
					var selectedNodes = selected.OfType<TNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					TNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<TNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					TNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) {
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None) {
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		private void CreateDefaultDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) => {
					var selectedNodes = selected.OfType<TNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return false;
					}
					TNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete;
				},
				(selected) => true,
				(selected) => {
					var selectedNodes = selected.OfType<TNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1) {
						return;
					}
					TNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType)) {
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete) {
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				}
			);
			NodeActionsList.Add(deleteAction);
		}

		private void HideJournal(ITdiTabParent parenTab)
		{
			if(TabParent is ITdiSliderTab slider) {
				slider.IsHideJournal = true;
			}
		}

		#endregion Actions
	}
}
