using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Project.Services;
using QS.Services;

namespace QS.ViewModels
{
	public class EntityJournalActionsViewModel : ButtonsJournalActionsViewModel
	{
		private Type entityType;
		private Action createEntityDialogAction;
		private Action<object> editEntityDialogAction;
		private readonly IDeleteEntityService deleteEntityService;

		public EntityJournalActionsViewModel(
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
		{
			CurrentPermissionService = currentPermissionService;
			this.deleteEntityService = deleteEntityService;
		}

		public ICurrentPermissionService CurrentPermissionService { get; }

		#region Дефолтные значения экшена Добавить

		protected override bool CanCreateEntity()
		{
			return CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanCreate;
		}

		protected override void DefaultAddAction()
		{
			createEntityDialogAction?.Invoke();
		}
		
		#endregion

		#region Дефолтные значения экшена Изменить
		
		protected override bool CanEditEntity()
		{
			return (CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanUpdate)
			       && SelectedItems.Any();
		}

		protected override void DefaultEditAction()
		{
			foreach(var item in SelectedItems)
			{
				editEntityDialogAction?.Invoke(item);
			}
		}
		
		#endregion
		
		#region Дефолтные значения экшена Удалить

		protected override bool CanDeleteEntity()
		{
			return (CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(entityType).CanDelete)
			       && SelectedItems.Any();
		}

		protected override void DefaultDeleteAction()
		{
			foreach(var item in SelectedItems)
			{
				deleteEntityService.DeleteEntity(entityType, DomainHelper.GetId(item));
			}
		}
		
		#endregion

		public virtual void Initialize(
			Type entityType,
			Action createEntityDialogAction,
			Action<object> editEntityDialogAction,
			bool createSelectAction = true,
			bool createAddAction = true,
			bool createEditAction = true,
			bool createDeleteAction = true)
		{
			this.entityType = entityType;
			this.createEntityDialogAction = createEntityDialogAction;
			this.editEntityDialogAction = editEntityDialogAction;
			
			CreateDefaultSelectAction(createSelectAction);
			CreateActions(createAddAction, createEditAction, createDeleteAction);
		}
	}
}