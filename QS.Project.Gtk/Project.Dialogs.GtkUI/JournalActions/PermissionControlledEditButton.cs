using QS.RepresentationModel.GtkUI;
using System.Linq;
using QS.Services;

namespace QS.Project.Dialogs.GtkUI.JournalActions
{
	public class PermissionControlledEditButton : RepresentationEditButton
	{
		private readonly IPermissionResult permission;
		private readonly bool _customEdit;
		
		public PermissionControlledEditButton(
			IJournalDialog dialog,
			IRepresentationModel representationModel,
			IPermissionResult permission,
			bool customEdit = false) : base(dialog, representationModel)
		{
			this.permission = permission;
			_customEdit = customEdit;
		}

		public override void CheckSensitive(object[] selected)
		{
			Button.Sensitive = (_customEdit ? permission.CanRead : permission.CanUpdate) && selected.Any();
		}
	}
}
