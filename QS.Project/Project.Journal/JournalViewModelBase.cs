using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;
using NHibernate.Criterion;
using NLog;
using QS.Dialog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.Actions;
using QS.Project.Journal.Actions.ViewModels;
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

		public ILifetimeScope AutofacScope { get; set; }

		protected JournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IJournalActionsViewModel journalActionsViewModel = null
		) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			JournalActions = journalActionsViewModel;
			SelectedItems = new List<object>();
			PopupActionsList = new List<IJournalAction>();

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += Search_OnSearch;
			searchHelper = new SearchHelper(Search);

			UseSlider = false;
		}

		#region Данные
		public IDataLoader DataLoader { get; protected set; }

		public IList Items => DataLoader.Items;

		public void Refresh()
		{
			DataLoader.LoadData(false);
		}
		#endregion

		public virtual string FooterInfo {
			get => $"Загружено: {DataLoader.Items.Count} шт.";
			set { }
		}

		#region Дочерние ViewModels
		public virtual IJournalFilterViewModel JournalFilter { get; protected set; }

		private IJournalActionsViewModel journalActions;
		public IJournalActionsViewModel JournalActions {
			get => journalActions;
			protected set {
				journalActions = value;
				if (journalActions != null)
					journalActions.MyJournal = this;
			}
		}

		#endregion

		#region Popup
		public virtual IEnumerable<IJournalAction> PopupActions => PopupActionsList;
		protected virtual List<IJournalAction> PopupActionsList { get; set; }
		#endregion

		#region Выделение
		private JournalSelectionMode selectionMode;
		public virtual JournalSelectionMode SelectionMode {
			get => selectionMode;
			set => SetField(ref selectionMode, value);
		}

		private IList<object> selectedItems;

		public IList<object> SelectedItems {
			get => selectedItems;
			set {
				if (SetField(ref selectedItems, value)) {
					if (JournalActions is IJournalEventsHandler eventsHandler) {
						eventsHandler.OnSelectionChanged(selectedItems);
					}
				}
			}
		}
		#endregion
		#region Режим выбора в журнале
		public event EventHandler<JournalSelectedEventArgs> OnSelectResult;

		public bool ChoiceMode {
			get => (JournalActions as IChoiceActionsViewModel)?.EnableChoiceMode ?? false;
			set {
				if (JournalActions is IChoiceActionsViewModel choice)
					choice.EnableChoiceMode = value;
			} 
		}

		public virtual void OnItemsSelected()
		{
			OnSelectResult?.Invoke(this, new JournalSelectedEventArgs(SelectedItems));
			Close(false, CloseSource.Self);
		}
		#endregion

		#region ITDIJournal implementation
		/// <summary>
		/// Устанавливает использовать ли систему вложенных вкладок
		/// </summary>
		public virtual bool? UseSlider { get; protected set; }
		#endregion

		#region ISlideableViewModel

		bool ISlideableViewModel.UseSlider => UseSlider ?? false;

		public bool AlwaysNewPage { get; protected set; }

		#endregion

		public void UpdateOnChanges(params Type[] entityTypes)
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnEntitiesUpdated, entityTypes);
		}

		private void OnEntitiesUpdated(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		#region Поиск

		public virtual IJournalSearch Search { get; set; }

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

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);

			if (JournalActions is IDisposable disposableJournalActionsViewModel) {
				disposableJournalActionsViewModel.Dispose();
			}

			base.Dispose();
		}
	}
}
