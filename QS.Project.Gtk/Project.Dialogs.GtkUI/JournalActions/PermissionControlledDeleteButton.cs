using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.RepresentationModel.GtkUI;
using System.Linq;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledDeleteButton : RepresentationDeleteButton
	{
		private readonly EntityPermission permission;

		public PermissionControlledDeleteButton(IJournalDialog dialog, IRepresentationModel representationModel, EntityPermission permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.Delete && selected.Any();
		}
	}
}
