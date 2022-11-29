using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using NHibernate.Criterion;
using NLog;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class JournalViewModelBase : UoWTabViewModelBase, ITdiJournal, IAutofacScopeHolder, ISlideableViewModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public virtual IJournalFilterViewModel JournalFilter { get; protected set; }

		public virtual IJournalSearch Search { get; set; }

		public virtual bool SearchEnabled { get; set; } = true;
		
		public IDataLoader DataLoader { get; protected set; }

		public IList Items => DataLoader.Items;

		public virtual string FooterInfo {
			get => $"Загружено: {DataLoader.Items.Count} шт.";
			set { }
		}

		public virtual IEnumerable<IJournalAction> NodeActions => NodeActionsList;
		protected virtual List<IJournalAction> NodeActionsList { get; set; }

		public virtual IEnumerable<IJournalAction> PopupActions => PopupActionsList;
		protected virtual List<IJournalAction> PopupActionsList { get; set; }

		public virtual IJournalAction RowActivatedAction { get; protected set; }

		public void Refresh()
		{
			DataLoader.LoadData(false);
		}

		private JournalSelectionMode selectionMode;
		public virtual JournalSelectionMode SelectionMode {
			get => selectionMode;
			set {
				if(SetField(ref selectionMode, value, () => SelectionMode)) {
					CreateNodeActions();
				}
			}
		}

		public Action UpdateJournalActions;
		public event EventHandler<JournalSelectedEventArgs> OnSelectResult;

		#region ITDIJournal implementation
		/// <summary>
		/// Устанавливает использовать ли систему вложенных вкладок
		/// </summary>
		public virtual bool? UseSlider { get; protected set; }
		#endregion

		public ILifetimeScope AutofacScope { get; set; }

		#region ISlideableViewModel

		bool ISlideableViewModel.UseSlider => UseSlider ?? false;

		public bool AlwaysNewPage { get; protected set; } = true;

		#endregion

		protected JournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			NodeActionsList = new List<IJournalAction>();
			PopupActionsList = new List<IJournalAction>();

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += Search_OnSearch;
			searchHelper = new SearchHelper(Search);

			UseSlider = false;
		}

		protected virtual void OnItemsSelected(object[] selectedNodes, bool closeJournal = true)
		{
			OnSelectResult?.Invoke(this, new JournalSelectedEventArgs(selectedNodes));
            if(closeJournal)
            {
				Close(false, CloseSource.Self);
			}
		}

		#region Configure actions

		protected virtual void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected virtual void CreatePopupActions()
		{
		}

		protected virtual void CreateDefaultSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any(),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple) {
				RowActivatedAction = selectAction;
			}
			NodeActionsList.Add(selectAction);
		}

		#endregion Configure actions

		public void UpdateOnChanges(params Type[] entityTypes)
		{
			NotifyConfiguration.Instance?.UnsubscribeAll(this);
			NotifyConfiguration.Instance?.BatchSubscribeOnEntity(OnEntitiesUpdated, entityTypes);
		}

		private void OnEntitiesUpdated(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		public override void Dispose()
		{
			NotifyConfiguration.Instance?.UnsubscribeAll(this);
			base.Dispose();
		}

		#region Поиск

		void Search_OnSearch(object sender, EventArgs e)
		{
			Refresh();
		}

		private readonly SearchHelper searchHelper;

		protected ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			return searchHelper.GetSearchCriterion(aliasPropertiesExpr);
		}

		protected ICriterion GetSearchCriterion<TRootEntity>(params Expression<Func<TRootEntity, object>>[] propertiesExpr)
		{
			return searchHelper.GetSearchCriterion(propertiesExpr);
		}

		#endregion
	}
}
