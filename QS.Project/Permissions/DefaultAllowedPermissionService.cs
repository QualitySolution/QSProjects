using System;
using QS.Services;
using QS.DomainModel.Entity.EntityPermissions;
namespace QS.Permissions
{
	public class DefaultAllowedPermissionService : IPermissionService
	{
		public IPermissionResult ValidateEntityPermissionForCurrentUser(Type entityType)
		{
			return new PermissionResult(EntityPermission.AllAllowed);
		}

		public IPermissionResult ValidateEntityPermissionForUser(Type entityType, int userId)
		{
			return new PermissionResult(EntityPermission.AllAllowed);
		}

		public bool ValidatePresetPermissionForCurrentUser(string permissionName)
		{
			return true;
		}

		public bool ValidatePresetPermissionForUser(string permissionName, int userId)
		{
			return true;
		}
	}
}
