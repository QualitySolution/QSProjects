using System;

namespace QS.DomainModel.Entity.EntityPermissions
{
	public interface IEntityPermissionValidator
	{
		EntityPermission Validate(Type entityType, int userId);
		EntityPermission Validate<TEntityType>(int userId);
	}
}
