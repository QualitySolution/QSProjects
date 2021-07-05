using System;
using System.Linq;
using QS.Commands;
using QS.Project.Journal;
using QS.Tdi;

namespace QS.ViewModels
{
	public class JournalActionsViewModel : UoWWidgetViewModelBase
	{
		private bool isSelectVisible;
		protected object[] selectedItems = new object[0];

		protected JournalSelectionMode SelectionMode { get; set; }
		protected Action OnItemsSelectedAction { get; set; }
		public bool CanSelect => CanSelectFunc != null && CanSelectFunc.Invoke();
		public virtual object[] SelectedItems
		{
			get => selectedItems;
			set
			{
				if(SetField(ref selectedItems, value))
				{
					OnPropertyChanged(nameof(CanSelect));
				}
			}
		}

		public DelegateCommand SelectCommand { get; private set; }
		public ITdiTabParent TabParent { get; set; }
		public bool IsSelectVisible
		{
			get => isSelectVisible;
			set => SetField(ref isSelectVisible, value);
		}
		
		public Action RowActivatedAction { get; set; }
		public Action SelectAction { get; set; }
		public Func<bool> CanSelectFunc { get; set; }

		protected virtual void InitializeRowActivatedAction()
		{
			if (SelectionMode != JournalSelectionMode.None) 
			{
				RowActivatedAction = SelectAction; 
			}
		}
		
		protected virtual void DefaultSelectAction()
		{
			OnItemsSelectedAction?.Invoke();
		}
		
		public virtual bool CanSelectEntity()
		{
			return SelectedItems != null && SelectedItems.Any();
		}
		
		public virtual void CreateDefaultSelectAction(
			JournalSelectionMode selectionMode,
			Action onItemsSelected,
			bool createSelectAction = true)
		{
			SelectionMode = selectionMode;
			OnItemsSelectedAction = onItemsSelected;

			CreateSelectAction(createSelectAction);
		}

		private void CreateSelectAction(bool createSelectAction)
		{
			if(createSelectAction)
			{
				SelectAction = DefaultSelectAction;
				CanSelectFunc = CanSelectEntity;
				SelectCommand = new DelegateCommand(SelectAction, () => CanSelect);
				IsSelectVisible = SelectionMode != JournalSelectionMode.None;
				InitializeRowActivatedAction();
			}
			else
			{
				IsSelectVisible = false;
			}
		}
	}
}