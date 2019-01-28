using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.Project.Repositories
{
	public static class EntityUserPermissionRepository
	{
		public static EntityUserPermission GetUserPermission(IUnitOfWork uow, string entityName, int userId)
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

		public static IList<EntityUserPermission> GetUserAllPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<EntityUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}
	}
}
