using System;
using System.Collections.Generic;
using System.Linq;
using QS.ViewModels;

namespace QS.Project.Journal.Actions.ViewModels
{
	public class JournalActionsViewModelBase<TNode> : ViewModelBase, IJournalEventsHandler, IJournalActionsViewModel
	{
		#region Свойства
		public JournalViewModelBase MyJournal { get; set; }
		#endregion

		#region IJournalEventsHandler
		void IJournalEventsHandler.OnCellDoubleClick(object node, object columnTag, object subcolumnTag)
		{
			OnCellDoubleClick((TNode)node, columnTag, subcolumnTag);
		}

		void IJournalEventsHandler.OnSelectionChanged(IList<object> nodes)
		{
			OnSelectionChanged(nodes.Cast<TNode>().ToArray());
		}
		#endregion

		#region События во View журнала

		protected virtual void OnCellDoubleClick(TNode node, object columnTag, object subcolumnTag)
		{

		}

		protected virtual void OnSelectionChanged(IList<TNode> nodes)
		{
			selectedItems = nodes;
		}

		public virtual void OnKeyPressed(string key)
		{

		}

		#endregion

		#region Выделение
		protected IList<TNode> selectedItems;
		#endregion

		//FIXME Ниже под удаление.

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
			var selectAction = new JournalAction(
				"Выбрать",
				CanSelect,
				() => SelectionMode != JournalSelectionMode.None,
				DefaultSelectAction,
				ActionType.Select
			);
			
			JournalActions.Add(selectAction);
		}
	}
}