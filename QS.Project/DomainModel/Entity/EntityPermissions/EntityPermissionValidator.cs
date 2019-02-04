using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions
{
	public class EntityPermissionValidator : IEntityPermissionValidator
	{
		public virtual bool NoRestrictPermissionsForAdmin { get; set; } = true;

		public virtual EntityPermission Validate(Type entityType, int userId)
		{
			var permissionAttr = entityType.GetCustomAttribute<EntityPermissionAttribute>();
			if(permissionAttr == null) {
				return new EntityPermission(false, false, false, false);
			}
			UserBase user;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				user = UserRepository.GetUserById(uow, userId);
			}

			if(user != null && user.IsAdmin && NoRestrictPermissionsForAdmin) {
				return new EntityPermission(true, true, true, true);
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
