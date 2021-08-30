using System;
using System.Linq;
using QS.ViewModels;

namespace QS.Project.Journal.Actions.ViewModels
{
	public abstract class ButtonsJournalActionsViewModel : JournalActionsViewModel
	{
		#region Дефолтные значения экшена Добавить
		
		protected virtual string DefaultAddLabel() => "Добавить";

		protected abstract bool CanCreateEntity();

		protected abstract void DefaultAddAction();

		#endregion

		#region Дефолтные значения экшена Изменить

		protected virtual string DefaultEditLabel() => "Изменить";

		protected abstract bool CanEditEntity();

		protected abstract void DefaultEditAction();

		#endregion

		#region Дефолтные значения экшена Удалить

		protected virtual string DefaultDeleteLabel() => "Удалить";

		protected abstract bool CanDeleteEntity();

		protected abstract void DefaultDeleteAction();
		
		#endregion
		
		#region Создание экшенов
		
		protected void CreateActions(bool createAddAction, bool createEditAction, bool createDeleteAction)
		{
			CreateAddAction(createAddAction);
			CreateEditAction(createEditAction);
			CreateDeleteAction(createDeleteAction);
		}

		#region Создание экшена Добавить

		protected virtual void CreateAddAction(bool createAddAction)
		{
			if(createAddAction)
			{
				CreateAction(DefaultAddLabel(), CanCreateEntity, () => true, DefaultAddAction, ActionType.Add, "Insert");
			}
		}

		#endregion

		#region Создание экшена Изменить

		private void CreateEditAction(bool createEditAction)
		{
			if(createEditAction)
			{
				CreateAction(DefaultEditLabel(), CanEditEntity, () => true, DefaultEditAction, ActionType.Edit);
				InitializeRowActivatedAction();
			}
		}
		
		#endregion

		#region Создание экшена Удалить

		private void CreateDeleteAction(bool createDeleteAction)
		{
			if(createDeleteAction)
			{
				CreateAction(DefaultDeleteLabel(), CanDeleteEntity, () => true, DefaultDeleteAction, ActionType.Delete, "Delete");
			}
		}
		
		#endregion
		
		protected void CreateAction(
			string label,
			Func<bool> sensitiveFunc,
			Func<bool> visibleFunc,
			Action executeAction,
			ActionType actionType,
			string hotkeys = null)
		{
			var action = new DefaultJournalAction(label, sensitiveFunc, visibleFunc, executeAction, actionType, hotkeys);
			JournalActions.Add(action);
		}
		
		#endregion
		
		protected override void InitializeRowActivatedAction()
		{
			if(JournalActions.Any())
			{
				var editAction = JournalActions.SingleOrDefault(a => a.ActionType == ActionType.Edit);

				if(editAction != null)
				{
					if(SelectionMode == JournalSelectionMode.None)
					{
						RowActivatedAction = editAction.ExecuteAction;
					}
				}
				
				base.InitializeRowActivatedAction();
			}
		}
	}
}