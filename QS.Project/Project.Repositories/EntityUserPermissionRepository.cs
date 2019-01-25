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
	}
}
