using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NLog;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.DataLoader;
using QS.Project.Search;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;

namespace QS.Project.Journal
{
	public abstract class JournalViewModelBase : UoWTabViewModelBase, ITdiJournal, IAutofacScopeHolder
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public abstract Type NodeType { get; }

		public virtual IJournalFilter Filter { get; protected set; }

		public virtual IJournalSearch Search { get; set; }

		public IDataLoader DataLoader { get; protected set; }

		public IList Items => DataLoader.Items;

		public virtual string FooterInfo => $"Загружено: {DataLoader.Items.Count} шт.";

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

		public event EventHandler<JournalSelectedEventArgs> OnSelectResult;

		#region ITDIJournal implementation
		public virtual bool? UseSlider { get; protected set; }
		#endregion

		public ILifetimeScope AutofacScope { get; set; }

		protected JournalViewModelBase(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService) : base(unitOfWorkFactory, interactiveService)
		{
			NodeActionsList = new List<IJournalAction>();
			PopupActionsList = new List<IJournalAction>();

			Search = new SearchViewModel(interactiveService);

			UseSlider = false;
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
			base.Dispose();
		}
	}
}
