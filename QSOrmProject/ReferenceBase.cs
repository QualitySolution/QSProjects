using System;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Services;

namespace QS
{
	public class ReferenceBase : Bin
	{
		protected IPermissionResult permissionResult { get; set; }
		protected virtual System.Type objectType { get; set; }
		public virtual bool FailInitialize { get; private set; }

		public virtual IUnitOfWork UoW { get; set; }

		public event EventHandler TabClosed;

		protected void InitializePermissionValidator()
		{
			var user = UserRepository.GetCurrentUser(UoW);
			permissionResult = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(objectType, user.Id);

			if(!permissionResult.CanRead) {
				var message = PermissionsSettings.GetEntityReadValidateResult(objectType);
				MessageDialogHelper.RunErrorDialog(message);
				FailInitialize = true;
			}
		}

		public void OnTabClosed()
		{
			TabClosed?.Invoke(this, EventArgs.Empty);
		}

	}
}
