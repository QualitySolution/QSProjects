using System;
using System.Collections;
using System.Collections.Generic;
using QS.Services;
using QS.ViewModels;
using QS.DomainModel.UoW;
using System.Linq;
using QS.Project.Search;
using QS.Tdi;

namespace QS.Project.Journal
{
	public abstract class JournalViewModelBase : UoWTabViewModelBase, ITdiJournal
	{
		public abstract Type NodeType { get; }

		public virtual IJournalFilter Filter { get; protected set; }

		public virtual IJournalSearch Search { get; set; }

		public virtual IList Items { get; protected set; }

		public event EventHandler ItemsListUpdated;

		public virtual string FooterInfo => $"Загружено: {Items.Count} шт.";

		public virtual IEnumerable<IJournalAction> NodeActions => NodeActionsList;
		protected virtual List<IJournalAction> NodeActionsList { get; set; }

		public virtual IEnumerable<IJournalAction> PopupActions => PopupActionsList;
		protected virtual List<IJournalAction> PopupActionsList { get; set; }

		public virtual IJournalAction RowActivatedAction { get; protected set; }

		public bool DynamicLoadingEnabled {  get; protected set; }

		public abstract void Refresh();

		public abstract bool TryLoad();

		private JournalSelectionMode selectionMode;
		public virtual JournalSelectionMode SelectionMode {
			get => selectionMode;
			set {
				if(SetField(ref selectionMode, value, () => SelectionMode)) {
					CreateNodeActions();
				}
			}
		}

		public event EventHandler<JournalSelectedEventArgs> OnSelectResult;

		#region ITDIJournal implementation

		public virtual bool? UseSlider { get; protected set; }

		#endregion

		protected JournalViewModelBase(IInteractiveService interactiveService) : base(interactiveService)
		{
			Items = new List<object>();
			NodeActionsList = new List<IJournalAction>();
			PopupActionsList = new List<IJournalAction>();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			Search = new SearchViewModel(interactiveService);
			SelectionMode = JournalSelectionMode.None;

			UseSlider = false;
			DynamicLoadingEnabled = false;
		}

		protected virtual void UpdateItems(IList items)
		{
			Items = items;
			RaiseItemsUpdated();
		}

		protected virtual void RaiseItemsUpdated()
		{
			ItemsListUpdated?.Invoke(this, EventArgs.Empty);
		}

		internal virtual void OnItemsSelected(object[] selectedNodes)
		{
			OnSelectResult?.Invoke(this, new JournalSelectedEventArgs(selectedNodes));
			Close(false);
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
				OnItemsSelected
			);
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple) {
				RowActivatedAction = selectAction;
			}
			NodeActionsList.Add(selectAction);
		}

		#endregion Configure actions



		public override bool HasChanges => false;

		public override void SaveAndClose()
		{
		}

		public override bool Save()
		{
			return false;
		}
	}
}
