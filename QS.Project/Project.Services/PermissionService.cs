using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.PresetPermissions;
namespace QS.Services
{
	public class PermissionService : IPermissionService
	{
		readonly IEntityPermissionValidator permissionValidator;
		readonly IPresetPermissionValidator presetPermissionValidator;

		public PermissionService(IEntityPermissionValidator entityPermissionValidator, IPresetPermissionValidator presetPermissionValidator)
		{
			this.presetPermissionValidator = presetPermissionValidator ?? throw new ArgumentNullException(nameof(presetPermissionValidator));
			this.permissionValidator = entityPermissionValidator ?? throw new ArgumentNullException(nameof(entityPermissionValidator));
		}

		public IPermissionResult ValidateUserPermission(Type entityType, int userId) => new PermissionResult(permissionValidator.Validate(entityType, userId));

		public bool ValidateUserPresetPermission(string permissionName, int userId) => presetPermissionValidator.Validate(permissionName, userId);
	}

	public class PermissionResult : IPermissionResult
	{
		private readonly EntityPermission entityPermission;

		public bool CanCreate => entityPermission.Create;
		public bool CanRead => entityPermission.Read;
		public bool CanUpdate => entityPermission.Update;
		public bool CanDelete => entityPermission.Delete;

		public PermissionResult(EntityPermission entityPermission)
		{
			this.entityPermission = entityPermission;
		}
	}
}