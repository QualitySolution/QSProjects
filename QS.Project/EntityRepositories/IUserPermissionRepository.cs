using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.EntityRepositories
{
	public interface IUserPermissionRepository
	{
		IReadOnlyDictionary<string, bool> CurrentUserPresetPermissions { get; }

		IList<EntityUserPermission> GetUserAllEntityPermissions(IUnitOfWork uow, int userId);
		IList<PresetUserPermission> GetUserAllPresetPermissions(IUnitOfWork uow, int userId);
		EntityUserPermission GetUserEntityPermission(IUnitOfWork uow, string entityName, int userId);
	}
}