using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.EntityRepositories
{
	public class UserPermissionSingletonRepository : IUserPermissionRepository
	{
		private static UserPermissionSingletonRepository instance;

		public static UserPermissionSingletonRepository GetInstance()
		{
			if(instance == null)
				instance = new UserPermissionSingletonRepository();
			return instance;
		}

		protected UserPermissionSingletonRepository() { }

		public EntityUserPermission GetUserEntityPermission(IUnitOfWork uow, string entityName, int userId)
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

		public IEnumerable<UserPermissionNode> GetUserAllEntityPermissions(IUnitOfWork uow, int userId, IPermissionExtensionStore permissionExtensionStore)
		{
			var entityPermissionList = uow.Session.QueryOver<EntityUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();

			foreach(var item in entityPermissionList) {
				var node = new UserPermissionNode();
				node.EntityUserOnlyPermission = item;
				node.TypeOfEntity = item.TypeOfEntity;
				node.EntityPermissionExtended = new List<EntityUserPermissionExtended>();
				foreach(var extension in permissionExtensionStore.PermissionExtensions) {
					EntityUserPermissionExtended permissionExtendedAlias = null;

					var permission = uow.Session.QueryOver(() => permissionExtendedAlias)
						.Where(x => x.User.Id == userId)
						.And(() => permissionExtendedAlias.PermissionId == extension.PermissionId)
						.And(x => x.TypeOfEntity.Id == node.TypeOfEntity.Id)
						.Take(1)?.List()?.FirstOrDefault();

					if(permission != null) {
						node.EntityPermissionExtended.Add(permission);
						continue;
					}

					permission = new EntityUserPermissionExtended();
					permission.IsPermissionAvailable = null;
					permission.PermissionId = extension.PermissionId;
					permission.User = item.User;
					permission.TypeOfEntity = item.TypeOfEntity;
					node.EntityPermissionExtended.Add(permission);
				}

				yield return node;
			}
		}

		public IList<PresetUserPermission> GetUserAllPresetPermissions(IUnitOfWork uow, int userId)
		{
			return uow.Session.QueryOver<PresetUserPermission>()
				.Where(x => x.User.Id == userId)
				.List();
		}
	}
}
