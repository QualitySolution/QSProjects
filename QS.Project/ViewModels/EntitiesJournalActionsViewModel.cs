using System;
using System.Collections.Generic;
using System.Linq;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Tdi;

namespace QS.ViewModels
{
	public class EntitiesJournalActionsViewModel : JournalActionsViewModel
	{
		private Action hideJournalAction;
		protected IReadOnlyDictionary<Type, JournalEntityConfig> entityConfigs;
		protected ITdiTab journalTab;
		protected readonly IInteractiveService interactiveService;

		public EntitiesJournalActionsViewModel(IInteractiveService interactiveService)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public override object[] SelectedItems 
		{
			get => selectedItems;
			set 
			{
				if (SetField(ref selectedItems, value)) 
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

		protected virtual bool CanCreateEntity()
		{
			var entityConfig = entityConfigs.First().Value;
			return entityConfig.PermissionResult.CanCreate;
		}

		protected virtual void DefaultAddAction()
		{
			var entityConfig = entityConfigs.First().Value;
			var docConfig = entityConfig.EntityDocumentConfigurations.First();
			ITdiTab newTab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction();

			if (newTab is ITdiDialog dlg) 
			{
				dlg.EntitySaved += OnNewEntitySaved;
			}

			TabParent.OpenTab(() => newTab, journalTab);

			if (docConfig.JournalParameters.HideJournalForCreateDialog) 
			{
				hideJournalAction?.Invoke();
			}
		}

		protected virtual bool CanEditEntity()
		{
			if (SelectedItems == null) {
				return false;
			}

			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>().ToArray();

			if (selectedNodes.Length != 1) 
			{
				return false;
			}

			var selectedNode = selectedNodes.First();

			if (!entityConfigs.ContainsKey(selectedNode.EntityType)) 
			{
				return false;
			}

			var config = entityConfigs[selectedNode.EntityType];
			return config.PermissionResult.CanUpdate;
		}

		protected virtual void DefaultEditAction()
		{
			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>().ToArray();
			var selectedNode = selectedNodes.First();
			var config = entityConfigs[selectedNode.EntityType];
			var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

			TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), journalTab);

			if (foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) 
			{
				hideJournalAction?.Invoke();
			}
		}

		protected virtual bool CanDeleteEntity()
		{
			if (SelectedItems == null) {
				return false;
			}

			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>().ToArray();

			if (selectedNodes.Length != 1)
			{
				return false;
			}

			var selectedNode = selectedNodes.First();

			if (!entityConfigs.ContainsKey(selectedNode.EntityType)) 
			{
				return false;
			}

			var config = entityConfigs[selectedNode.EntityType];
			return config.PermissionResult.CanDelete;
		}

		protected virtual void DefaultDeleteAction()
		{
			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>();
			var selectedNode = selectedNodes.First();
			var config = entityConfigs[selectedNode.EntityType];

			if (config.PermissionResult.CanDelete)
			{
				DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
			}
		}

		private void OnNewEntitySaved(object sender, EntitySavedEventArgs e)
		{
			if (!(e?.Entity is IDomainObject))
			{
				return;
			}

			if (SelectionMode == JournalSelectionMode.None)
			{
				return;
			}

			if (SelectedItems == null)
			{
				return;
			}

			if (interactiveService.Question("Выбрать созданный объект и вернуться к предыдущему диалогу?"))
			{
				OnItemsSelectedAction?.Invoke();
			}
		}

		private void CreateAddActions(bool createAddAction)
		{
			if(createAddAction)
			{
				var totalCreateDialogConfigs = entityConfigs
					.Where(x => x.Value.PermissionResult.CanCreate)
					.Sum(x => x.Value.EntityDocumentConfigurations
						.Select(y => y.GetCreateEntityDlgConfigs().Count())
						.Sum());

				if(entityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
				{
					CreateMultipleAddActions();
				}
				else
				{
					CreateSingleAddAction();
				}
			}
		}

		private void CreateSingleAddAction()
		{
			CreateAction("Добавить", CanCreateEntity, () => true, DefaultAddAction, ActionType.Add);
		}

		private void CreateAction(
			string label,
			Func<bool> sensitiveFunc,
			Func<bool> visibleFunc,
			Action executeAction,
			ActionType actionType,
			string hotkeys = null)
		{
			var action = new ButtonElement(label, sensitiveFunc, visibleFunc, executeAction, actionType, hotkeys);
			JournalActions.Add(action);
		}

		private void CreateMultipleAddActions()
		{
			var addParentNodeAction =
				new ButtonElement("Добавить", () => true, () => true, () => { }, ActionType.MultipleAdd);

			foreach(var entityConfig in entityConfigs.Values)
			{
				foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
				{
					foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
					{
						var childNodeAction = new ButtonElement(createDlgConfig.Title,
							() => entityConfig.PermissionResult.CanCreate,
							() => entityConfig.PermissionResult.CanCreate,
							() =>
							{
								TabParent.OpenTab(createDlgConfig.OpenEntityDialogFunction, journalTab);

								if(documentConfig.JournalParameters.HideJournalForCreateDialog)
								{
									hideJournalAction?.Invoke();
								}
							},
							ActionType.Add
						);
						addParentNodeAction.ChildButtonElements.Add(childNodeAction);
					}
				}
			}

			JournalActions.Add(addParentNodeAction);
		}

		private void CreateActions(bool createAddAction, bool createEditAction, bool createDeleteAction)
		{
			CreateAddActions(createAddAction);
			CreateEditAction(createEditAction);
			CreateDeleteAction(createDeleteAction);
		}

		private void CreateEditAction(bool createEditAction)
		{
			if(createEditAction)
			{
				CreateAction("Изменить", CanEditEntity, () => true, DefaultEditAction, ActionType.Edit);
			}
		}

		private void UpdateRowActivatedAction()
		{
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = JournalActions.SingleOrDefault(a => a.ActionType == ActionType.Delete)?.ExecuteAction;
			}
		}

		private void CreateDeleteAction(bool createDeleteAction)
		{
			if(createDeleteAction)
			{
				CreateAction("Удалить", CanDeleteEntity, () => true, DefaultDeleteAction, ActionType.Delete);
			}
		}

		public virtual void Initialize(
			JournalSelectionMode selectionMode,
			Dictionary<Type, JournalEntityConfig> entityConfigs,
			ITdiTab journalTab,
			Action hideJournal,
			Action onItemsSelected,
			bool createSelectAction = true,
			bool createAddAction = true,
			bool createEditAction = true,
			bool createDeleteAction = true)
		{
			this.entityConfigs = entityConfigs;
			this.journalTab = journalTab;
			hideJournalAction = hideJournal;
			
			CreateDefaultSelectAction(selectionMode, onItemsSelected, createSelectAction);
			CreateActions(createAddAction, createEditAction, createDeleteAction);
		}
	}
}