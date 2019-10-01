using System;
using QS.RepresentationModel.GtkUI;
using QS.Services;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledAddButton : RepresentationAddButton
	{
		private readonly IPermissionResult permission;

		public PermissionControlledAddButton(IJournalDialog dialog, IRepresentationModel representationModel, IPermissionResult permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.CanCreate;
		}
	}
}
