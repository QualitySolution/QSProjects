using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Repositories;

namespace QS.DomainModel.Entity.EntityPermissions
{
	public class EntityPermissionValidator : IEntityPermissionValidator
	{
		public virtual EntityPermission Validate(Type entityType, int userId)
		{
			var permissionAttr = entityType.GetCustomAttribute<EntityPermissionAttribute>();
			if(permissionAttr == null) {
				return new EntityPermission(false, false, false, false);
			}

			var userPermission = UserPermissionRepository.GetUserEntityPermission(UnitOfWorkFactory.CreateWithoutRoot(), entityType.Name, userId);
			if(userPermission == null) {
				return EntityPermission.Empty;
			} else {
				return new EntityPermission(
					userPermission.CanCreate,
					userPermission.CanRead,
					userPermission.CanUpdate,
					userPermission.CanDelete
				);
			}
		}

		public virtual EntityPermission Validate<TEntityType>(int userId)
		{
			return Validate(typeof(TEntityType), userId);
		}
	}
}
