using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.PresetPermissions
{
	public class PresetPermissionValidator : IPresetPermissionValidator
	{
		public bool Validate(string presetPermissionName, int userId)
		{
			PresetUserPermission perm;

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				perm = uow.Session.QueryOver<PresetUserPermission>()
				.Where(x => x.User.Id == userId && x.PermissionName == presetPermissionName)
				.Take(1)
				.SingleOrDefault();
			}

			return perm != null;
		}
	}
}