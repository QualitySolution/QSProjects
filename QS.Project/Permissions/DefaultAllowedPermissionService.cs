using System;
using QS.Services;
using QS.DomainModel.Entity.EntityPermissions;
namespace QS.Permissions
{
	public class DefaultAllowedPermissionService : IPermissionService, ICurrentPermissionService
	{
		public IPermissionResult ValidateEntityPermission(Type entityType)
		{
			return new PermissionResult(EntityPermission.AllAllowed);
		}

		public bool ValidatePresetPermission(string permissionName)
		{
			return true;
		}

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
