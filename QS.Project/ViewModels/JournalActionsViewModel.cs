using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Tdi;

namespace QS.ViewModels
{
	public class JournalActionsViewModel : WidgetViewModelBase
	{
		protected object[] selectedItems = new object[0];

		private JournalSelectionMode selectionMode;
		public JournalSelectionMode SelectionMode
		{
			get => selectionMode;
			set
			{
				if(SetField(ref selectionMode, value))
				{
					if(JournalActions.Any())
					{
						foreach(var action in JournalActions)
						{
							OnPropertyChanged(nameof(action.Visible));
						}
					}
				}
			}
		}

		protected Action OnItemsSelectedAction { get; set; }
		
		public virtual object[] SelectedItems
		{
			get => selectedItems;
			set
			{
				if(SetField(ref selectedItems, value))
				{
					if(JournalActions.Any())
					{
						foreach(var action in JournalActions)
						{
							OnPropertyChanged(nameof(action.Sensitive));
						}
					}
				}
			}
		}

		public ITdiTabParent TabParent { get; set; }
		
		public Action RowActivatedAction { get; set; }
		
		public IList<ButtonElement> JournalActions { get; set; } = new List<ButtonElement>();

		protected virtual void InitializeRowActivatedAction()
		{
			if (SelectionMode != JournalSelectionMode.None) 
			{
				if(JournalActions.Any())
				{
					RowActivatedAction =
						JournalActions.SingleOrDefault(a => a.ActionType == ActionType.Select)?.ExecuteAction; 
				}
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
				CreateAction();
				InitializeRowActivatedAction();
			}
		}

		private void CreateAction()
		{
			var selectAction = new ButtonElement(
				"Выбрать",
				CanSelectEntity,
				() => SelectionMode != JournalSelectionMode.None,
				OnItemsSelectedAction,
				ActionType.Select
			);
			
			JournalActions.Add(selectAction);
		}
	}

	public class ButtonElement : PropertyChangedBase
	{
		private string label;
		
		public ButtonElement(
			string label,
			Func<bool> sensitiveFunc,
			Func<bool> visibleFunc,
			Action executeAction,
			ActionType actionType,
			string hotkeys = null)
		{
			SensitiveFunc = sensitiveFunc;
			VisibleFunc = visibleFunc;
			Label = label;
			ExecuteAction = executeAction;
			ActionType = actionType;
			HotKeys = hotkeys;
		}

		public Func<bool> SensitiveFunc { get; set; }
		public Func<bool> VisibleFunc { get; set; }

		public bool Sensitive => SensitiveFunc != null && SensitiveFunc.Invoke();
		public bool Visible => VisibleFunc != null && VisibleFunc.Invoke();

		public string Label
		{
			get => label;
			set => SetField(ref label, value);
		}

		public Action ExecuteAction { get; set; }
		public IList<ButtonElement> ChildButtonElements { get; set; } = new List<ButtonElement>();
		public ActionType ActionType { get; }
		public string HotKeys { get; }

		public new void OnPropertyChanged(string propertyName = "")
		{
			base.OnPropertyChanged(propertyName);
		}
	}

	public enum ActionType
	{
		Select,
		Add,
		MultipleAdd,
		Edit,
		Delete,
		Custom
	}
}