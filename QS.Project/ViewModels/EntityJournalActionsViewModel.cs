using System;
using System.Linq;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;

namespace QS.ViewModels
{
	public class EntityJournalActionsViewModel : JournalActionsViewModel, IJournalCommands
	{
		private Type entityType;
		private Action CreateEntityDialogAction;
		private Action<object> EditEntityDialogAction;
		private Action editAction; 
		
		protected IDeleteEntityService DeleteEntityService;

		public EntityJournalActionsViewModel(
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
		{
			CurrentPermissionService = currentPermissionService;
			DeleteEntityService = deleteEntityService;
		}

		public ICurrentPermissionService CurrentPermissionService { get; set; }
		
		public bool CanCreate => CanCreateFunc != null && CanCreateFunc.Invoke();
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
					OnPropertyChanged(nameof(CanCreate));
					OnPropertyChanged(nameof(CanEdit));
					OnPropertyChanged(nameof(CanDelete));
				}
			}
		}

		public DelegateCommand AddCommand { get; private set; }
		public DelegateCommand EditCommand { get; private set; }
		public DelegateCommand DeleteCommand { get; private set; }

		public bool IsAddVisible { get; set; } = true;
		public bool IsEditVisible { get; set; } = true;
		public bool IsDeleteVisible { get; set; } = true;
		
		protected virtual bool CanCreateEntity()
		{
			return CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanCreate;
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
		public Func<bool> CanCreateFunc { get; set; }
		public Func<bool> CanEditFunc { get; set; }
		public Func<bool> CanDeleteFunc { get; set; }

		protected virtual void DefaultAddAction()
		{
			CreateEntityDialogAction?.Invoke();
		}

		protected virtual bool CanEditEntity()
		{
			return (CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanUpdate)
			       && SelectedItems.Any();
		}

		protected virtual void DefaultEditAction()
		{
			foreach(var item in SelectedItems)
			{
				EditEntityDialogAction?.Invoke(item);
			}
		}

		protected virtual bool CanDeleteEntity()
		{
			return (CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanDelete)
			       && SelectedItems.Any();
		}

		protected virtual void DefaultDeleteAction()
		{
			foreach(var item in SelectedItems)
			{
				DeleteEntityService.DeleteEntity(entityType, DomainHelper.GetId(item));
			}
		}

		private void CreateAddAction(bool createAddAction)
		{
			if(createAddAction && IsAddVisible)
			{
				CreateDefaultAddAction();
			}
			else
			{
				IsAddVisible = false;
			}
		}

		private void CreateDefaultAddAction()
		{
			CanCreateFunc = CanCreateEntity;
			AddAction = DefaultAddAction;
			AddCommand = new DelegateCommand(() => AddAction?.Invoke(), () => CanCreate);
		}
		
		private void CreateActions(bool createAddAction, bool createEditAction, bool createDeleteAction)
		{
			CreateAddAction(createAddAction);
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
			Type entityType,
			JournalSelectionMode selectionMode,
			Action onItemsSelected,
			Action createEntityDialogAction,
			Action<object> editEntityDialogAction,
			bool createSelectAction = true,
			bool createAddAction = true,
			bool createEditAction = true,
			bool createDeleteAction = true)
		{
			this.entityType = entityType;
			CreateEntityDialogAction = createEntityDialogAction;
			EditEntityDialogAction = editEntityDialogAction;
			
			CreateDefaultSelectAction(selectionMode, onItemsSelected, createSelectAction);
			CreateActions(createAddAction, createEditAction, createDeleteAction);
		}
	}
}