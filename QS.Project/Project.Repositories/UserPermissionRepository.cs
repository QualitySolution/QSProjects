using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.Project.Repositories
{
	public static class UserPermissionRepository
	{
		public static EntityUserPermission GetUserEntityPermission(IUnitOfWork uow, string entityName, int userId)
		{
			TypeOfEntity typeOfEntityAlias = null;
			EntityUserPermission entityUserPermissionAlias = null;
			UserBase userBaseAlias = null;
			return uow.Session.QueryOver<EntityUserPermission>(() => entityUserPermissionAlias)
				.Left.JoinAlias(() => entityUserPermissionAlias.User, () => userBaseAlias)
				.Left.JoinAlias(() => entityUserPermissionAlias.TypeOfEntity, () => typeOfEntityAlias)
				.Where(() => userBaseAlias.Id == userId)
				.Where(() => typeOfEntityAlias.Type == entityName)
				.SingleOrDefault();
		}

		public static IList<EntityUserPermission> GetUserAllEntityPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntityUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}

		public static IList<PresetUserPermission> GetUserAllPresetPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<PresetUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}
	}
}
