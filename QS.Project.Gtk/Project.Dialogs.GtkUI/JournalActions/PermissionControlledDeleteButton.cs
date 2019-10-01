using System;
using QS.RepresentationModel.GtkUI;
using System.Linq;
using QS.Services;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledDeleteButton : RepresentationDeleteButton
	{
		private readonly IPermissionResult permission;

		public PermissionControlledDeleteButton(IJournalDialog dialog, IRepresentationModel representationModel, IPermissionResult permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.CanDelete && selected.Any();
		}
	}
}
