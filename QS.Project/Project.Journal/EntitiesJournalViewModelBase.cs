using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NHibernate;
using NHibernate.Util;
using QS.Deletion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;

namespace QS.Project.Journal
{
	public abstract class EntitiesJournalViewModelBase<TNode> : JournalViewModelBase
		where TNode : JournalEntityNodeBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private readonly ICommonServices commonServices;

		protected Dictionary<Type, JournalEntityConfig<TNode>> EntityConfigs { get; private set; }

		private IJournalFilter filter;
		public IJournalFilter Filter {
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

		//NavigationManager navigation = null - чтобы не переделывать классов в Водовозе, где будет использоваться передадут.
		protected EntitiesJournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, INavigationManager navigation = null) : base(unitOfWorkFactory, commonServices?.InteractiveService, navigation)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UseSlider = true;
			EntityConfigs = new Dictionary<Type, JournalEntityConfig<TNode>>();
		}

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		internal override void OnItemsSelected(params object[] selectedNodes)
		{
			OnEntitySelectedResult.Invoke(this, new JournalSelectedNodesEventArgs(selectedNodes.Cast<JournalEntityNodeBase>().ToArray()));
			Close(false, CloseSource.Self);
		}

		void Tab_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			if(e?.Entity == null)
				return;
			if(!(e.Entity is IDomainObject))
				return;
			if(SelectionMode == JournalSelectionMode.None)
				return;

			TNode node = DataLoader.GetNodes((e.Entity as IDomainObject).Id, UoW)
						.OfType<TNode>()
						.FirstOrDefault(x => x.EntityType == e.Entity.GetType());

			if(node == null)
				return;
			if(AskQuestion("Выбрать созданный объект и вернуться к предыдущему диалогу?"))
				OnItemsSelected(new object[] { node });
		}

		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity<TEntity>(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunction)
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

		#region Ordering

		[Obsolete("Метод оставлен для совместимости со старым подходом к настройке загрузки. Желательно для новых журналов настраивать DataLoader напрямую.")]
		protected void SetOrder(Func<TNode, object> orderFunc, bool desc = false)
		{
			var threadLoader = DataLoader as ThreadDataLoader<TNode>;
			threadLoader.ShowLateResults = false;
			if(threadLoader == null)
				throw new InvalidCastException($"Метод поддерживает только загрузчик по умолчанию {nameof(ThreadDataLoader<TNode>)}, для всех остальных случаев настраивайте DataLoader напрямую.");

			threadLoader.MergeInOrderBy(orderFunc, desc);
		}

		#endregion Ordering

		#region Entity load configuration

		[Obsolete("Метод оставлен для совместимости со старым подходом к настройке. Желательно для новых журналов настраивать DataLoader напрямую.")]
		private void CreateLoader<TEntity>(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunc)
			where TEntity : class, IDomainObject
		{
			if (DataLoader == null)
				DataLoader = new ThreadDataLoader<TNode>(UnitOfWorkFactory);

			var threadLoader = DataLoader as ThreadDataLoader<TNode>;
			if (threadLoader == null)
				throw new InvalidCastException($"Метод поддерживает только загрузчик по умолчанию {nameof(ThreadDataLoader<TNode>)}, для всех остальных случаев настраивайте DataLoader напрямую.");
			threadLoader.ShowLateResults = false;
			//HACK Здесь добавляем адаптер для совместимости со старой настройкой. Не берите с этого места пример. Так делать не надо. Так сделано только чтобы не перепысывать все старые журналы в водовозе. Надесь этот метот целиком в будущем удалим.
			if(commonServices.PermissionService != null && commonServices.UserService != null)
				threadLoader.CurrentPermissionService = new CurrentPermissionServiceAdapter(commonServices.PermissionService, commonServices.UserService);

			threadLoader.AddQuery<TEntity>(queryFunc);
		}


		#endregion Entity load configuration

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

		protected void CreateDefaultAddActions()
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

						if(tab is ITdiDialog)
							((ITdiDialog)tab).EntitySaved += Tab_EntitySaved;

						TabParent.OpenTab(() => tab, this);
						if(docConfig.JournalParameters.HideJournalForCreateDialog) {
							HideJournal(TabParent);
						}
					});
				NodeActionsList.Add(addAction);
			};
		}

		protected void CreateDefaultEditAction()
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

		protected void CreateDefaultDeleteAction()
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
