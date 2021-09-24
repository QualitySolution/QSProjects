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
		
		private IList<object> selectedItems;
		public IList<object> SelectedItems
		{
			get => selectedItems; 
			set
			{
				if(SetField(ref selectedItems, value))
				{
					if(JournalActionsViewModel != null)
					{
						JournalActionsViewModel.SelectedItems = SelectedItems;
					}
				}
			}
		}

		public virtual IJournalSearch Search { get; set; }

		public IDataLoader DataLoader { get; protected set; }

		public IList Items => DataLoader.Items;

		public virtual string FooterInfo {
			get => $"Загружено: {DataLoader.Items.Count} шт.";
			set { }
		}

		#region Дочерние ViewModels
		public virtual IJournalFilterViewModel JournalFilter { get; protected set; }

		private JournalActionsViewModel journalActionsViewModel;
		public JournalActionsViewModel JournalActionsViewModel {
			get => journalActionsViewModel;
			protected set {
				journalActionsViewModel = value;
				if(journalActionsViewModel != null)
					journalActionsViewModel.MyJournal = this;
			}
		}

		#endregion
		public virtual IEnumerable<IJournalAction> PopupActions => PopupActionsList;
		protected virtual List<IJournalAction> PopupActionsList { get; set; }

		public void Refresh()
		{
			DataLoader.LoadData(false);
		}

		private JournalSelectionMode selectionMode;
		public virtual JournalSelectionMode SelectionMode 
		{
			get => selectionMode;
			set 
			{
				if(SetField(ref selectionMode, value)) 
				{
					JournalActionsViewModel.SelectionMode = SelectionMode;
				}
			}
		}

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

		public bool AlwaysNewPage { get; protected set; }

		#endregion

		protected JournalViewModelBase(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			JournalActionsViewModel journalActionsViewModel = null
		) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			JournalActionsViewModel = journalActionsViewModel;
			SelectedItems = new List<object>();
			PopupActionsList = new List<IJournalAction>();

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += Search_OnSearch;
			searchHelper = new SearchHelper(Search);

			UseSlider = false;
			//FIXME Предусмотреть что она может быть пустой
			JournalActionsViewModel.OnItemsSelectedAction += OnItemsSelected;
		}

		protected virtual void OnItemsSelected()
		{
			OnSelectResult?.Invoke(this, new JournalSelectedEventArgs(SelectedItems));
			Close(false, CloseSource.Self);
		}

		#region Configure actions

		protected virtual void CreatePopupActions()
		{
		}

		#endregion Configure actions

		public void UpdateOnChanges(params Type[] entityTypes)
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity(OnEntitiesUpdated, entityTypes);
		}

		private void OnEntitiesUpdated(EntityChangeEvent[] changeEvents)
		{
			Refresh();
		}

		public override void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			JournalActionsViewModel.OnItemsSelectedAction -= OnItemsSelected;

			if(JournalActionsViewModel is IDisposable disposableJournalActionsViewModel)
			{
				disposableJournalActionsViewModel.Dispose();
			}
			
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
