using System;
using System.Reflection;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QS.Static;
using QS.Tdi;

namespace QS
{
	public class ReferenceBase : Bin
	{
		protected EntityPermission entityPermissions { get; set; }
		protected virtual System.Type objectType { get; set; }
		public virtual bool FailInitialize { get; private set; }

		public virtual IUnitOfWork UoW { get; set; }

		protected void InitializePermissionValidator()
		{
			if(PermissionsMain.EntityPermissionValidator == null) {
				return;
			}
			var user = UserRepository.GetCurrentUser(UoW);
			entityPermissions = PermissionsMain.EntityPermissionValidator.Validate(objectType, user.Id);

			if(!entityPermissions.Read) {
				var message = PermissionsMain.GetEntityReadValidateResult(objectType);
				MessageDialogHelper.RunErrorDialog(message);
				FailInitialize = true;
			}
		}
	}
}
