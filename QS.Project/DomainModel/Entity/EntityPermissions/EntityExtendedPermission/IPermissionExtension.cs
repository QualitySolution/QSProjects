using System;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission {
public interface IPermissionExtension
	{
		string PermissionId { get; }

		string Name { get; }

		string Description { get; }

		bool IsValidType(Type typeOfEntity);
	}
}
