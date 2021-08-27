using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class EntitiesJournalViewModelBase<TNode> : JournalViewModelBase, IEntityAutocompleteSelector
		where TNode : JournalEntityNodeBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		protected readonly ICommonServices CommonServices;

		protected Dictionary<Type, JournalEntityConfig> EntityConfigs { get; private set; }

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
		
		public EntitiesJournalActionsViewModel EntitiesJournalActionsViewModel { get; }

		public event EventHandler<JournalSelectedNodesEventArgs> OnEntitySelectedResult;
		public event EventHandler ListUpdated;

		//NavigationManager navigation = null - чтобы не переделывать классов в Водовозе, где будет использоваться передадут.
		
		protected EntitiesJournalViewModelBase(
			JournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation = null)
			: base(journalActionsViewModel, unitOfWorkFactory, commonServices?.InteractiveService, navigation)
		{
			if(journalActionsViewModel is EntitiesJournalActionsViewModel entitiesJournalActionsViewModel)
			{
				EntitiesJournalActionsViewModel = entitiesJournalActionsViewModel;
			}
			
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UseSlider = true;
			EntityConfigs = new Dictionary<Type, JournalEntityConfig>();
		}

		void FilterViewModel_OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override void OnItemsSelected()
		{
			OnEntitySelectedResult?.Invoke(this, new JournalSelectedNodesEventArgs(SelectedItems.Cast<JournalEntityNodeBase>().ToArray()));
			Close(false, CloseSource.Self);
		}

		protected JournalEntityConfigurator<TEntity> RegisterEntity<TEntity>(Func<IUnitOfWork, IQueryOver<TEntity>> queryFunction)
		where TEntity : class, IDomainObject, INotifyPropertyChanged, new()
		{
			if(queryFunction == null) {
				throw new ArgumentNullException(nameof(queryFunction));
			}

			CreateLoader(queryFunction);

			var configurator = new JournalEntityConfigurator<TEntity>();
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
			InitializeJournalActionsViewModel();
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
			if(CommonServices.PermissionService != null && CommonServices.UserService != null)
				threadLoader.CurrentPermissionService = new CurrentPermissionServiceAdapter(CommonServices.PermissionService, CommonServices.UserService);

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
			IPermissionResult entityPermissionResult = CommonServices.PermissionService.ValidateUserPermission(entityType, CommonServices.UserService.CurrentUserId);

			if(EntityConfigs.ContainsKey(entityType)) {
				EntityConfigs[entityType].PermissionResult = entityPermissionResult;
			}
		}

		#endregion Permissions

		#region Actions

		protected override void InitializeJournalActionsViewModel()
		{
			if(EntitiesJournalActionsViewModel != null)
			{
				EntitiesJournalActionsViewModel.Initialize(EntityConfigs, this, HideJournal);
			}
			else
			{
				base.InitializeJournalActionsViewModel();
			}
		}

		protected override void CreatePopupActions()
		{
		}
		
		protected void HideJournal()
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
	}
}
