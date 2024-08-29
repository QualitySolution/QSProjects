using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace QS.DomainModel.Entity.PresetPermissions
{
	public class PresetPermissionValidator : IPresetPermissionValidator
	{
		private readonly IUnitOfWorkFactory uowFactory;

		public PresetPermissionValidator(IUnitOfWorkFactory uowFactory) {
			this.uowFactory = uowFactory ?? throw new System.ArgumentNullException(nameof(uowFactory));
		}

		public bool Validate(string presetPermissionName, int userId)
		{
			PresetUserPermission perm;

			using(var uow = uowFactory.CreateWithoutRoot()) {
				perm = uow.Session.QueryOver<PresetUserPermission>()
				.Where(x => x.User.Id == userId && x.PermissionName == presetPermissionName)
				.Take(1)
				.SingleOrDefault();
			}

			return perm != null;
		}
	}
}
