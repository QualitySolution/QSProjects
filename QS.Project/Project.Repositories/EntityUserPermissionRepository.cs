using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.Project.Repositories
{
	public static class EntityUserPermissionRepository
	{
		public static EntityUserPermission GetUserPermission(IUnitOfWork uow, string entityName, int userId)
		{
			return uow.Session.QueryOver<EntityUserPermission>()
				.Where(x => x.User.Id == userId)
				.Where(x => x.EntityName == entityName)
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
