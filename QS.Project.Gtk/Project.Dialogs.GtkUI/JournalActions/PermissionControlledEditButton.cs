using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledEditButton : RepresentationEditButton
	{
		private readonly EntityPermission permission;

		public PermissionControlledEditButton(IJournalDialog dialog, IRepresentationModel representationModel, EntityPermission permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.Update;
		}
	}
}
