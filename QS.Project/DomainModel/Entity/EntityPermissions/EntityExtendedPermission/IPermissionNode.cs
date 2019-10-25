using System.Collections.Generic;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public interface IPermissionNode
	{
		TypeOfEntity TypeOfEntity { get; set; }

		IList<EntityPermissionExtendedBase> EntityPermissionExtended { get; set; }

		EntityPermissionBase EntityPermission { get; }
	}
}