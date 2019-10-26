using System.Collections.Generic;

namespace QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission
{
	public interface IPermissionExtensionStore
	{
		IList<IPermissionExtension> PermissionExtensions { get; }
	}
}