using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NHibernate;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Journal;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels;

namespace QS.Project.Journal
{
	/// <summary>
	/// Базовый класс для журналов разрабатываемых внутри ВВ используется с завязкой на TDI поэтому не переносим в dotnet. 
	/// </summary>
	public abstract class EntitiesJournalViewModelBase<TNode> : UowJournalViewModelBase, ITdiJournal, IEntityAutocompleteSelector
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

		public virtual bool CanOpen(Type subjectType)
		{
			return !EntityConfigs.TryGetValue(subjectType, out var config)
				? throw new InvalidOperationException($"Не найдена конфигурация для {subjectType.Name}")
				: config.PermissionResult.CanUpdate;
		}

		public virtual ITdiTab GetTabToOpen(Type subjectType, int subjectId)
		{
			var constructorInfo = typeof(TNode).GetConstructor(new Type[]{});
			var node = constructorInfo?.Invoke(new object[]{}) as TNode;
			if (node == null)
				return null;

			node.Id = subjectId;
			EntityConfigs.TryGetValue(subjectType, out var config);
			var foundDocumentConfig =
				(config ?? throw new InvalidOperationException($"Не найдена конфигурация для {subjectType.Name}"))
				.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(node));

			return (foundDocumentConfig?.GetOpenEntityDlgFunction()
			     ?? throw new InvalidOperationException(
				        $"Не найдена конфигурация для открытия диалога в {nameof(foundDocumentConfig)}"))
				.Invoke(node);
		}
		public event EventHandler ListUpdated;

		//NavigationManager navigation = null - чтобы не переделывать классов в Водовозе, где будет использоваться передадут.
		protected EntitiesJournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null,
			IEntityChangeWatcher entityChangeWatcher = null
			) : base(unitOfWorkFactory, navigation, entityChangeWatcher)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.interactiveService = this.commonServices.InteractiveService;
			UseSlider = true;
			EntityConfigs = new Dictionary<Type, JournalEntityConfig<TNode>>();
		}

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override void OnItemsSelected(object[] selectedNodes, bool closeJournal = true)
		{
			OnEntitySelectedResult?.Invoke(this, new JournalSelectedNodesEventArgs(selectedNodes.Cast<JournalEntityNodeBase>().ToArray()));
			base.OnItemsSelected(selectedNodes, closeJournal);
		}

		protected void Tab_EntitySaved(object sender, EntitySavedEventArgs e)
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
		
		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity<TEntity>(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunction, Func<IUnitOfWork, int> itemsCountFunction = null)
		where TEntity : class, IDomainObject, INotifyPropertyChanged
		{
			if(queryFunction == null) {
				throw new ArgumentNullException(nameof(queryFunction));
			}

			CreateLoader(queryFunction, itemsCountFunction);

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
		
		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity<TEntity>(
			Func<IUnitOfWork, int?, int?, IQueryOver<TEntity>> queryFunction,
			Func<IUnitOfWork, int> itemsCountFunction = null)
			where TEntity : class, IDomainObject, INotifyPropertyChanged
		{
			if(queryFunction == null) {
				throw new ArgumentNullException(nameof(queryFunction));
			}

			CreateLoader(queryFunction, itemsCountFunction);

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
		
		protected JournalEntityConfigurator<TEntity, TNode> RegisterEntity<TEntity>(IEnumerable<Func<IUnitOfWork, IQueryOver<TEntity>>> queryFunctions, Func<IUnitOfWork, int> itemsCountFunction = null)
			where TEntity : class, IDomainObject, INotifyPropertyChanged {
			if(!queryFunctions.Any()) {
				throw new ArgumentException("Нельзя зарегистрировать сущность без функций запросов", nameof(queryFunctions));
			}

			foreach(var queryFunction in queryFunctions) {
				CreateLoader(queryFunction, itemsCountFunction);
			}

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
		private void CreateLoader<TEntity>(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunc, Func<IUnitOfWork, int> itemsCountFunction = null)
			where TEntity : class, IDomainObject
		{
			var threadLoader = InitializeThreadDataLoader<TEntity>();
			threadLoader.AddQuery<TEntity>(queryFunc, itemsCountFunction);
		}
		
		private void CreateLoader<TEntity>(
			Func<IUnitOfWork, int?, int?, IQueryOver<TEntity>> queryFunc,
			Func<IUnitOfWork, int> itemsCountFunction = null)
			where TEntity : class, IDomainObject
		{
			var threadLoader = InitializeThreadDataLoader<TEntity>();
			threadLoader.AddQuery<TEntity>(queryFunc, itemsCountFunction);
		}

		private ThreadDataLoader<TNode> InitializeThreadDataLoader<TEntity>() where TEntity : class, IDomainObject
		{
			if(DataLoader == null)
				DataLoader = new ThreadDataLoader<TNode>(UnitOfWorkFactory);

			var threadLoader = DataLoader as ThreadDataLoader<TNode>;
			if(threadLoader == null)
				throw new InvalidCastException(
					$"Метод поддерживает только загрузчик по умолчанию {nameof(ThreadDataLoader<TNode>)}, для всех остальных случаев настраивайте DataLoader напрямую.");
			threadLoader.ShowLateResults = false;
			//HACK Здесь добавляем адаптер для совместимости со старой настройкой. Не берите с этого места пример. Так делать не надо. Так сделано только чтобы не переписывать все старые журналы в водовозе. Надеюсь этот метод целиком в будущем удалим.
			if(commonServices.PermissionService != null && commonServices.UserService != null)
				threadLoader.CurrentPermissionService =
					new CurrentPermissionServiceAdapter(commonServices.PermissionService, commonServices.UserService);
			return threadLoader;
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
					},
					"Insert"
					);
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
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		protected void HideJournal(ITdiTabParent parenTab)
		{
			if(TabParent is ITdiSliderTab slider) {
				slider.IsHideJournal = true;
			}
		}

		public void SearchValues(params string[] values)
		{
			Search.SearchValues = values;
		}

		#endregion Actions

		#region Код в этом блоке скопирован из TabViewModelBase так как C# не поддерживает множественное наследование

		public override string Title { get => TabName; set => TabName = value; }

		#region ITdiTab implementation

		public HandleSwitchIn HandleSwitchIn { get; private set; }
		public HandleSwitchOut HandleSwitchOut { get; private set; }

		private string tabName = string.Empty;

		/// <summary>
		/// Имя вкладки может быть автоматически получено из атрибута DisplayNameAttribute у класса диалога.
		/// </summary>
		public virtual string TabName {
			get {
				if(string.IsNullOrWhiteSpace(tabName)) {
					return GetType().GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;
				}
				return tabName;
			}
			set {
				if(tabName == value)
					return;
				tabName = value;
				OnTabNameChanged();
			}
		}

		public ITdiTabParent TabParent { set; get; }

		public bool FailInitialize { get; protected set; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler TabClosed;

		public virtual bool CompareHashName(string hashName)
		{
			return GenerateHashName(this.GetType()) == hashName;
		}

		#endregion

		/// <summary>
		/// Отменяет открытие вкладки
		/// </summary>
		/// <param name="message">Сообщение пользователю при отмене открытия</param>
		protected void AbortOpening(string message, string title = "Невозможно открыть вкладку")
		{
			ShowErrorMessage(message, title);
			AbortOpening();
		}

		/// <summary>
		/// Отменяет открытие вкладки
		/// </summary>
		protected void AbortOpening()
		{
			FailInitialize = true;
		}

		public static string GenerateHashName<TTab>() where TTab : TabViewModelBase
		{
			return GenerateHashName(typeof(TTab));
		}

		public static string GenerateHashName(Type tabType)
		{
			if(!typeof(TabViewModelBase).IsAssignableFrom(tabType))
				throw new ArgumentException($"Тип должен наследоваться от {nameof(TabViewModelBase)}", nameof(tabType));

			return string.Format("Tab_{0}", tabType.Name);
		}

		protected virtual void OnTabNameChanged()
		{
			TabNameChanged?.Invoke(this, new TdiTabNameChangedEventArgs(TabName));
			OnPropertyChanged(nameof(Title));
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(askSave)
				TabParent?.AskToCloseTab(this, source);
			else
				TabParent?.ForceCloseTab(this, source);
			
			base.Close(askSave, source);
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}

		#region Перенесено из ViewModelBase для поддержания обратной совместимости. умрет здесь вместе с TabViewModelBase

		private readonly IInteractiveService interactiveService;

		protected virtual void ShowInfoMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Info, message, title);
		}

		protected virtual void ShowWarningMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Warning, message, title);
		}

		protected virtual void ShowErrorMessage(string message, string title = null)
		{
			interactiveService.ShowMessage(ImportanceLevel.Error, message, title);
		}

		protected virtual bool AskQuestion(string question, string title = null)
		{
			return interactiveService.Question(question, title);
		}

		#endregion

		#endregion
	}
}
