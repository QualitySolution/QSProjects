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
	public class EntitiesJournalActionsViewModel : ButtonsJournalActionsViewModel
	{
		private Action hideJournalAction;
		protected IReadOnlyDictionary<Type, JournalEntityConfig> EntityConfigs;
		protected ITdiTab JournalTab;
		protected readonly IInteractiveService InteractiveService;

		public EntitiesJournalActionsViewModel(IInteractiveService interactiveService)
		{
			InteractiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		#region Дефолтные значения экшена Добавить
		
		protected override bool CanCreateEntity()
		{
			var entityConfig = EntityConfigs.First().Value;
			return entityConfig.PermissionResult.CanCreate;
		}

		protected override void DefaultAddAction()
		{
			var entityConfig = EntityConfigs.First().Value;
			var docConfig = entityConfig.EntityDocumentConfigurations.First();
			ITdiTab newTab = docConfig.GetCreateEntityDlgConfigs().First().OpenEntityDialogFunction();

			if (newTab is ITdiDialog dlg)
			{
				dlg.EntitySaved += OnNewEntitySaved;
			}

			JournalTab.TabParent.OpenTab(() => newTab, JournalTab);

			if (docConfig.JournalParameters.HideJournalForCreateDialog)
			{
				hideJournalAction?.Invoke();
			}
		}
		
		#endregion

		#region Дефолтные значения экшена Изменить

		protected override bool CanEditEntity()
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

			if (!EntityConfigs.ContainsKey(selectedNode.EntityType)) 
			{
				return false;
			}

			var config = EntityConfigs[selectedNode.EntityType];
			return config.PermissionResult.CanUpdate;
		}

		protected override void DefaultEditAction()
		{
			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>().ToArray();
			var selectedNode = selectedNodes.First();
			var config = EntityConfigs[selectedNode.EntityType];
			var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

			JournalTab.TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), JournalTab);

			if (foundDocumentConfig.JournalParameters.HideJournalForOpenDialog) 
			{
				hideJournalAction?.Invoke();
			}
		}
		
		#endregion

		#region Дефолтные значения экшена Удалить

		protected override bool CanDeleteEntity()
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

			if (!EntityConfigs.ContainsKey(selectedNode.EntityType)) 
			{
				return false;
			}

			var config = EntityConfigs[selectedNode.EntityType];
			return config.PermissionResult.CanDelete;
		}

		protected override void DefaultDeleteAction()
		{
			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>();
			var selectedNode = selectedNodes.First();
			var config = EntityConfigs[selectedNode.EntityType];

			if (config.PermissionResult.CanDelete)
			{
				DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
			}
		}
		
		#endregion

		private void OnNewEntitySaved(object sender, EntitySavedEventArgs e)
		{
			if (!(e?.Entity is IDomainObject entity))
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

			var node = (JournalTab as JournalViewModelBase).DataLoader.GetNodes(entity.Id)
				.OfType<JournalEntityNodeBase>()
				.FirstOrDefault(x => x.EntityType == e.Entity.GetType());

			if(node == null)
			{
				return;
			}

			if (InteractiveService.Question("Выбрать созданный объект и вернуться к предыдущему диалогу?"))
			{
				selectedItems.Add(node);
				OnItemsSelectedAction?.Invoke();
			}
		}

		#region Создание экшенов
		
		#region Создание экшена Добавить

		protected override void CreateAddAction(bool createAddAction)
		{
			if(createAddAction)
			{
				var totalCreateDialogConfigs = EntityConfigs
					.Where(x => x.Value.PermissionResult.CanCreate)
					.Sum(x => x.Value.EntityDocumentConfigurations
						.Select(y => y.GetCreateEntityDlgConfigs().Count())
						.Sum());

				if(EntityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
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
			CreateAction(DefaultAddLabel(), CanCreateEntity, () => true, DefaultAddAction, ActionType.Add, "Insert");
		}

		private void CreateMultipleAddActions()
		{
			var addParentNodeAction =
				new DefaultJournalAction(DefaultAddLabel(), () => true, () => true, () => { }, ActionType.MultipleAdd);

			foreach(var entityConfig in EntityConfigs.Values)
			{
				foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
				{
					foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
					{
						var childNodeAction = new DefaultJournalAction(createDlgConfig.Title,
							() => entityConfig.PermissionResult.CanCreate,
							() => entityConfig.PermissionResult.CanCreate,
							() =>
							{
								JournalTab.TabParent.OpenTab(createDlgConfig.OpenEntityDialogFunction, JournalTab);

								if(documentConfig.JournalParameters.HideJournalForCreateDialog)
								{
									hideJournalAction?.Invoke();
								}
							},
							ActionType.Add
						);
						addParentNodeAction.ChildDefaultJournalActions.Add(childNodeAction);
					}
				}
			}

			JournalActions.Add(addParentNodeAction);
		}
		
		#endregion

		#endregion

		public void Initialize(
			Dictionary<Type, JournalEntityConfig> entityConfigs,
			ITdiTab journalTab,
			Action hideJournal,
			bool createSelectAction = true,
			bool createAddAction = true,
			bool createEditAction = true,
			bool createDeleteAction = true)
		{
			EntityConfigs = entityConfigs;
			JournalTab = journalTab;
			hideJournalAction = hideJournal;
			
			CreateDefaultSelectAction(createSelectAction);
			CreateActions(createAddAction, createEditAction, createDeleteAction);
		}
	}
}