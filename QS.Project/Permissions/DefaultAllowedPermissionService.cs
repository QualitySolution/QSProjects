using System;
using QS.Services;
using QS.DomainModel.Entity.EntityPermissions;
namespace QS.Permissions
{
	public class DefaultAllowedPermissionService : IPermissionService
	{
		public IPermissionResult ValidateUserPermission(Type entityType, int userId)
		{
			return new PermissionResult(EntityPermission.AllAllowed);
		}

		public bool ValidateUserPresetPermission(string permissionName, int userId)
		{
			return true;
		}
	}
}
