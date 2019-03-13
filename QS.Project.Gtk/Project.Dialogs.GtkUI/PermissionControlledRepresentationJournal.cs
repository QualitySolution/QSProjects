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

		public PermissionControlledRepresentationJournal(IRepresentationModel representationModel) : base(representationModel)
		{
			if(RepresentationModel.EntityType != null) {
				UpdateUserEntityPermission();
				if(!currentUserPermissions.Read) {
					var message = PermissionsSettings.GetEntityReadValidateResult(RepresentationModel.EntityType);
					MessageDialogHelper.RunErrorDialog(message);
					FailInitialize = true;
				}
			}
		}

		protected override void ConfigureActionButtons()
		{
			if(RepresentationModel.EntityType == null) {
				return;
			}

			UpdateUserEntityPermission();

			var editButton = new PermissionControlledEditButton(this, RepresentationModel, currentUserPermissions);

			ActionButtons.Add(new PermissionControlledAddButton(this, RepresentationModel, currentUserPermissions));
			ActionButtons.Add(editButton);
			ActionButtons.Add(new PermissionControlledDeleteButton(this, RepresentationModel, currentUserPermissions));

			DoubleClickAction = editButton;
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
}
