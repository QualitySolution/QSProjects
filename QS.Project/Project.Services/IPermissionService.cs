using System;
using QS.Permissions;

namespace QS.Services
{
	public interface IPermissionService
	{
		IPermissionResult ValidateUserPermission(Type entityType, int userId);
		bool ValidateUserPresetPermission(string permissionName, int userId);
	}
}
