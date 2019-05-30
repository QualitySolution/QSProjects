using System;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Dialogs.GtkUI.JournalActions;
using QS.Project.Domain;
using QS.Project.Repositories;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI
{
	public class PermissionControlledRepresentationJournal : RepresentationJournalDialog
	{
		Logger logger = LogManager.GetCurrentClassLogger();

		private EntityPermission currentUserPermissions;

		public PermissionControlledRepresentationJournal(IRepresentationModel representationModel, Buttons buttons = Buttons.All) : base(representationModel)
		{
			if(RepresentationModel.EntityType != null) {
				UpdateUserEntityPermission();
				if(!currentUserPermissions.Read) {
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
				ActionButtons.Add(new PermissionControlledAddButton(this, RepresentationModel, currentUserPermissions));
			}
			if(buttons.HasFlag(Buttons.Edit) || buttons.HasFlag(Buttons.All)) {
				var editButton = new PermissionControlledEditButton(this, RepresentationModel, currentUserPermissions);
				ActionButtons.Add(editButton);
				DoubleClickAction = editButton;
			}
			if(buttons.HasFlag(Buttons.Delete) || buttons.HasFlag(Buttons.All)) {
				ActionButtons.Add(new PermissionControlledDeleteButton(this, RepresentationModel, currentUserPermissions));
			}

			CreateButtons();
		}

		private void UpdateUserEntityPermission()
		{
			if(!currentUserPermissions.IsEmpty) {
				return;
			}

			if(PermissionsSettings.EntityPermissionValidator == null || RepresentationModel.EntityType == null) {
				currentUserPermissions = EntityPermission.AllDenied;
			}
			UserBase user;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				user = UserRepository.GetCurrentUser(uow);
			}
			if(user == null) {
				logger.Warn("Не определен текущий пользователь, при проверке прав в журнале");
				currentUserPermissions = EntityPermission.AllDenied;
			}

			currentUserPermissions = PermissionsSettings.EntityPermissionValidator.Validate(RepresentationModel.EntityType, user.Id);
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
