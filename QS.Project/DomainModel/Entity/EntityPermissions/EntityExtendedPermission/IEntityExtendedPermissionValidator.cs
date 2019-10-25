using System;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public interface IEntityExtendedPermissionValidator
	{
		bool Validate(Type entityType, int userId, string PermissionId);
		bool Validate(Type entityType, int userId, IPermissionExtension permissionExtension);
	}
}