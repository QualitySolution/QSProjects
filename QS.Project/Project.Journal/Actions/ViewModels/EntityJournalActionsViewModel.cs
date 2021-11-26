using System;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;

namespace QS.Project.Journal.Actions.ViewModels
{
	public class EntityJournalActionsViewModel<TEntityViewModel, TEntity> : ButtonsJournalActionsViewModel
		where TEntityViewModel : DialogViewModelBase
	{
		public INavigationManager NavigationManager { get; }
		
		private Type entityType;
		private Action createEntityDialogAction;
		private Action<object> editEntityDialogAction;
		private readonly IDeleteEntityService deleteEntityService;
		private readonly ICurrentPermissionService currentPermissionService;

		public EntityJournalActionsViewModel(
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService = null,
			ICurrentPermissionService currentPermissionService = null)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			this.currentPermissionService = currentPermissionService;
			this.deleteEntityService = deleteEntityService;
			Initialize(typeof(TEntity), CreateEntityDialog, EditEntityDialog);
		}

		#region Дефолтные значения экшена Добавить

		protected override bool CanCreateEntity()
		{
			return currentPermissionService == null || currentPermissionService.ValidateEntityPermission(entityType).CanCreate;
		}

		protected override void DefaultAddAction()
		{
			createEntityDialogAction?.Invoke();
		}
		
		#endregion

		#region Дефолтные значения экшена Изменить
		
		protected override bool CanEditEntity()
		{
			return (currentPermissionService == null || currentPermissionService.ValidateEntityPermission(entityType).CanUpdate)
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
			return (currentPermissionService == null || currentPermissionService.ValidateEntityPermission(entityType).CanDelete)
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

		#region Предопределенные действия
		
		protected virtual void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(MyJournal, EntityUoWBuilder.ForCreate());
		}

		protected virtual void EditEntityDialog(object node)
		{
			NavigationManager.OpenViewModel<TEntityViewModel, IEntityUoWBuilder>(MyJournal, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
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