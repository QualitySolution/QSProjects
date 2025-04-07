using System;

namespace QS.Permissions
{
	public interface ICurrentPermissionService
	{
		IPermissionResult ValidateEntityPermission(Type entityType);
		bool ValidatePresetPermission(string permissionName);
	}
}
