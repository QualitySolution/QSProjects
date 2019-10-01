using System;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Dialogs.GtkUI.JournalActions;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.RepresentationModel.GtkUI;
using QS.Services;

namespace QS.Project.Dialogs.GtkUI
{
	public class PermissionControlledRepresentationJournal : RepresentationJournalDialog
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		private IPermissionResult permissionResult;

		public PermissionControlledRepresentationJournal(IRepresentationModel representationModel, Buttons buttons = Buttons.All) : base(representationModel)
		{
			if(RepresentationModel.EntityType != null) {
				UpdateUserEntityPermission();
				if(!permissionResult.CanRead) {
					var message = PermissionsSettings.GetEntityReadValidateResult(RepresentationModel.EntityType);
					MessageDialogHelper.RunErrorDialog(message);
					FailInitialize = true;
				}
			}

			this.buttons = buttons;
			ConfigureActionButtons();
		}

		private readonly Buttons buttons;

		protected override void ConfigureActionButtons()
		{
			if(RepresentationModel.EntityType == null) {
				return;
			}

			UpdateUserEntityPermission();
			ActionButtons.Clear();

			if(buttons.HasFlag(Buttons.Add) || buttons.HasFlag(Buttons.All)) {
				ActionButtons.Add(new PermissionControlledAddButton(this, RepresentationModel, permissionResult));
			}
			if(buttons.HasFlag(Buttons.Edit) || buttons.HasFlag(Buttons.All)) {
				var editButton = new PermissionControlledEditButton(this, RepresentationModel, permissionResult);
				ActionButtons.Add(editButton);
				DoubleClickAction = editButton;
			}
			if(buttons.HasFlag(Buttons.Delete) || buttons.HasFlag(Buttons.All)) {
				ActionButtons.Add(new PermissionControlledDeleteButton(this, RepresentationModel, permissionResult));
			}

			CreateButtons();
		}

		private void UpdateUserEntityPermission()
		{
			permissionResult = new PermissionResult(EntityPermission.AllAllowed);

			UserBase user;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				user = UserRepository.GetCurrentUser(uow);
			}
			if(user == null) {
				logger.Warn("Не определен текущий пользователь, при проверке прав в журнале");
				return;
			}

			permissionResult = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(RepresentationModel.EntityType, user.Id);
		}
	}

	[Flags]
	public enum Buttons
	{
		None = 0,
		Add = 1,
		Edit = 2,
		Delete = 4,
		All = 7,
	}
}
