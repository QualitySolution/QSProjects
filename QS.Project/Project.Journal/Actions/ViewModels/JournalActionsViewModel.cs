using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.ViewModels;

namespace QS.Project.Journal.Actions.ViewModels
{
	public class JournalActionsViewModel : WidgetViewModelBase
	{
		protected IList<object> selectedItems;

		private JournalSelectionMode selectionMode;
		public JournalSelectionMode SelectionMode
		{
			get => selectionMode;
			set
			{
				if(SetField(ref selectionMode, value))
				{
					InitializeRowActivatedAction();
					
					if(JournalActions.Any())
					{
						foreach(var action in JournalActions)
						{
							action.OnPropertyChanged(nameof(action.Visible));
						}
					}
				}
			}
		}

		public Action OnItemsSelectedAction;
		
		public virtual IList<object> SelectedItems
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
							action.OnPropertyChanged(nameof(action.Sensitive));
						}
					}
				}
			}
		}

		public Action RowActivatedAction { get; set; }
		
		public IList<DefaultJournalAction> JournalActions { get; set; } = new List<DefaultJournalAction>();

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

		#region Дефолтные значения экшена Выбрать
		
		protected virtual void DefaultSelectAction()
		{
			OnItemsSelectedAction?.Invoke();
		}
		
		public virtual bool CanSelect()
		{
			return SelectedItems != null && SelectedItems.Any();
		}
		
		#endregion
		
		public void CreateDefaultSelectAction(bool createSelectAction = true)
		{
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
			var selectAction = new DefaultJournalAction(
				"Выбрать",
				CanSelect,
				() => SelectionMode != JournalSelectionMode.None,
				DefaultSelectAction,
				ActionType.Select
			);
			
			JournalActions.Add(selectAction);
		}
	}

	public class DefaultJournalAction : PropertyChangedBase
	{
		private string label;
		
		public DefaultJournalAction(
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
			ChildDefaultJournalActions = new List<DefaultJournalAction>();
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
		public IList<DefaultJournalAction> ChildDefaultJournalActions { get; }
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