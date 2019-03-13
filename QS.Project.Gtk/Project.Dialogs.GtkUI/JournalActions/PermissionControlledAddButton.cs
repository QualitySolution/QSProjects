using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.RepresentationModel.GtkUI;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledAddButton : RepresentationAddButton
	{
		private readonly EntityPermission permission;

		public PermissionControlledAddButton(IJournalDialog dialog, IRepresentationModel representationModel, EntityPermission permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.Create;
		}
	}
}
