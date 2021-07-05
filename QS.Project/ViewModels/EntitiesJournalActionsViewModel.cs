using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Deletion;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;

namespace QS.ViewModels
{
	public class EntitiesJournalActionsViewModel : JournalActionsViewModel, IJournalCommands
	{
		protected IReadOnlyDictionary<Type, JournalEntityConfig> entityConfigs;
		protected ITdiTab journalTab;
		protected readonly IInteractiveService interactiveService;
		private Action editAction;

		public EntitiesJournalActionsViewModel(IInteractiveService interactiveService)
		{
			this.interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}

		public bool CanAdd => CanAddFunc != null && CanAddFunc.Invoke();
		public bool CanEdit => CanEditFunc != null && CanEditFunc.Invoke();
		public bool CanDelete => CanDeleteFunc != null && CanDeleteFunc.Invoke();

		public override object[] SelectedItems 
		{
			get => selectedItems;
			set 
			{
				if (SetField(ref selectedItems, value)) 
				{
					OnPropertyChanged(nameof(CanSelect));
					OnPropertyChanged(nameof(CanAdd));
					OnPropertyChanged(nameof(CanEdit));
					OnPropertyChanged(nameof(CanDelete));
				}
			}
		}

		public DelegateCommand AddCommand { get; private set; }
		public IJournalAction AddJournalActions { get; private set; }
		public DelegateCommand EditCommand { get; private set; }
		public DelegateCommand DeleteCommand { get; private set; }

		public bool IsAddVisible { get; set; } = true;
		public bool IsEditVisible { get; set; } = true;
		public bool IsDeleteVisible { get; set; } = true;

		private Action hideJournalAction;

		protected virtual bool CanCreateEntity()
		{
			var entityConfig = entityConfigs.First().Value;
			return entityConfig.PermissionResult.CanCreate;
		}

		public Action AddAction { get; set; }

		public Action EditAction
		{
			get => editAction;
			set
			{
				if(editAction == value)
				{
					return;
				}

				editAction = value;
				UpdateRowActivatedAction();
			}
		}
		public Action DeleteAction { get; set; }
		public Func<bool> CanAddFunc { get; set; }
		public Func<bool> CanEditFunc { get; set; }
		public Func<bool> CanDeleteFunc { get; set; }

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
			if(createAddAction && IsAddVisible)
			{
				CreateDefaultAddActions();
			}
			else
			{
				IsAddVisible = false;
			}
		}

		private void CreateDefaultAddActions()
		{
			var totalCreateDialogConfigs = entityConfigs
				.Where(x => x.Value.PermissionResult.CanCreate)
				.Sum(x => x.Value.EntityDocumentConfigurations
					.Select(y => y.GetCreateEntityDlgConfigs().Count())
					.Sum());

			if (entityConfigs.Values.Count(x => x.PermissionResult.CanRead) > 1 || totalCreateDialogConfigs > 1)
			{
				var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });

				foreach (var entityConfig in entityConfigs.Values)
				{
					foreach (var documentConfig in entityConfig.EntityDocumentConfigurations)
					{
						foreach (var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
						{
							var childNodeAction = new JournalAction(createDlgConfig.Title,
								selected => entityConfig.PermissionResult.CanCreate,
								selected => entityConfig.PermissionResult.CanCreate,
								selected => {
									TabParent.OpenTab(createDlgConfig.OpenEntityDialogFunction, journalTab);

									if (documentConfig.JournalParameters.HideJournalForCreateDialog)
									{
										hideJournalAction?.Invoke();
									}
								}
							);
							addParentNodeAction.ChildActionsList.Add(childNodeAction);
						}
					}
				}

				AddJournalActions = addParentNodeAction;
			}
			else 
			{
				CanAddFunc = CanCreateEntity;
				AddAction = DefaultAddAction;
				AddCommand = new DelegateCommand(() => AddAction?.Invoke(), () => CanAdd);
			}
		}
		
		private void CreateActions(bool createAddAction, bool createEditAction, bool createDeleteAction)
		{
			CreateAddActions(createAddAction);
			CreateEditAction(createEditAction);
			CreateDeleteAction(createDeleteAction);
		}

		private void CreateEditAction(bool createEditAction)
		{
			if(createEditAction && IsEditVisible)
			{
				CreateDefaultEditAction();
			}
			else
			{
				IsEditVisible = false;
			}
		}

		private void CreateDefaultEditAction()
		{
			CanEditFunc = CanEditEntity;
			EditAction = DefaultEditAction;
			EditCommand = new DelegateCommand(() => EditAction?.Invoke(), () => CanEdit);
		}

		private void UpdateRowActivatedAction()
		{
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = EditAction;
			}
		}

		private void CreateDeleteAction(bool createDeleteAction)
		{
			if(createDeleteAction && IsDeleteVisible)
			{
				CreateDefaultDeleteAction();
			}
			else
			{
				IsDeleteVisible = false;
			}
		}

		private void CreateDefaultDeleteAction()
		{
			CanDeleteFunc = CanDeleteEntity;
			DeleteAction = DefaultDeleteAction;
			DeleteCommand = new DelegateCommand(() => DeleteAction?.Invoke(), () => CanDelete);
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