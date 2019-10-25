using System.Collections.Generic;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.EntityRepositories
{
	public interface IUserPermissionRepository
	{
		IReadOnlyDictionary<string, bool> CurrentUserPresetPermissions { get; }

		IEnumerable<UserPermissionNode> GetUserAllEntityPermissions(IUnitOfWork uow, int userId, IPermissionExtensionStore permissionExtensionStore);
		IList<PresetUserPermission> GetUserAllPresetPermissions(IUnitOfWork uow, int userId);
		EntityUserPermission GetUserEntityPermission(IUnitOfWork uow, string entityName, int userId);
	}
}