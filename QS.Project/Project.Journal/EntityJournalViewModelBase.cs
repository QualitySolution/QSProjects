using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

		internal override void OnItemsSelected(object[] selectedNodes)
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
			LoadData(false);
		}

		#region Ordering

		protected void SetOrder(Func<TNode, object> orderFunc, bool desc = false)
		{
			orderingDictionary = new Dictionary<Func<TNode, object>, bool> { {orderFunc, desc } };
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

		protected int? GetPageSize => DynamicLoadingEnabled ? PageSize : (int?)null;

		public override bool FullDataLoaded => !entityLoaders.Any(l => l.Value.HasUnloadedItems);

		private bool reloadRequested = false;
		private Task[] RunningTasks = new Task[] { };
		private DateTime startLoading;

		public bool LoadInProgress = false;

		public override void LoadData(bool nextPage)
		{
			if(!EntityConfigs.Any()) 
				return;

			if(LoadInProgress) {
				if(!nextPage)
					reloadRequested = true;
				return;
			}

			logger.Info("Запрос данных...");
			startLoading = DateTime.Now;
			LoadInProgress = true;

			FirstPage = !nextPage;
			if(!nextPage) {
				Items.Clear();
				foreach(var item in entityLoaders.Values) {
					item.Reset();
				}
			}

			var runLoaders = entityLoaders.Values.Where(x => x.HasUnloadedItems).ToArray();
			RunningTasks = new Task[runLoaders.Length];

			for(int i = 0; i < runLoaders.Length; i++) {
				var loader = runLoaders[i];
				RunningTasks[i] = Task.Factory.StartNew(() => loader.LoadPage(GetPageSize));
			}

			Task.Factory.ContinueWhenAll(RunningTasks, (tasks) => {
				ReadLoadersInSortOrder();
				RaiseItemsUpdated();
				logger.Info($"{(DateTime.Now - startLoading).TotalSeconds} сек." );
				LoadInProgress = false;
				if(reloadRequested) {
					reloadRequested = false;
					LoadData(false);
				}
			});
		}

		private void ReadLoadersInSortOrder()
		{
			var loaders = entityLoaders.Values.ToList();
			var filtredLoaders = MakeOrderedEnumerable(loaders.Where(l => l.NextUnreadedNode() != null));
			while(loaders.Any(l => l.NextUnreadedNode() != null)) {
				if(loaders.Any(l => l.HasUnloadedItems && l.NextUnreadedNode() == null))
					break; //Уперлись в неподгруженный хвост. Пока хватит, ждем следующей страницы.
				var taked = filtredLoaders.First();
				Items.Add(taked.NextUnreadedNode());
				taked.ReadedItemsCount++;
			}
		}

		private IEnumerable<IEntityLoader<TNode>> MakeOrderedEnumerable(IEnumerable<IEntityLoader<TNode>> loaders)
		{
			if(orderingDictionary != null && orderingDictionary.Any()) {
				IOrderedEnumerable<IEntityLoader<TNode>> resultItems = null;
				bool isFirstValueInDictionary = true;
				foreach(var orderRule in orderingDictionary) {
					if(isFirstValueInDictionary) {
						if(orderRule.Value)
							resultItems = loaders.OrderByDescending(l => orderRule.Key(l.NextUnreadedNode()));
						else
							resultItems = loaders.OrderBy(l => orderRule.Key(l.NextUnreadedNode()));
						isFirstValueInDictionary = false;
					} else {
						if(orderRule.Value)
							resultItems = resultItems.ThenByDescending(l => orderRule.Key(l.NextUnreadedNode()));
						else
							resultItems = resultItems.ThenBy(l => orderRule.Key(l.NextUnreadedNode()));
					}
				}
				return resultItems;
			}

			return loaders;
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

			entityLoaders.Add(entityType, new DynamicEntityLoader<TEntity, TNode>(queryFunc));
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
