using System;
using QS.RepresentationModel.GtkUI;
using System.Linq;
using QS.Services;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledEditButton : RepresentationEditButton
	{
		private readonly IPermissionResult permission;

		public PermissionControlledEditButton(IJournalDialog dialog, IRepresentationModel representationModel, IPermissionResult permission) : base(dialog, representationModel)
		{
			this.permission = permission;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = permission.CanUpdate && selected.Any();
		}
	}
}
