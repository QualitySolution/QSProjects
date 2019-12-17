using System;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.PresetPermissions;
namespace QS.Services
{
	public class PermissionService : IPermissionService
	{
		private readonly IUserService userService;
		private readonly IEntityPermissionValidator permissionValidator;
		private readonly IPresetPermissionValidator presetPermissionValidator;

		public PermissionService(IUserService userService, IEntityPermissionValidator entityPermissionValidator, IPresetPermissionValidator presetPermissionValidator)
		{
			this.presetPermissionValidator = presetPermissionValidator ?? throw new ArgumentNullException(nameof(presetPermissionValidator));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.permissionValidator = entityPermissionValidator ?? throw new ArgumentNullException(nameof(entityPermissionValidator));
		}

		public IPermissionResult ValidateEntityPermissionForCurrentUser(Type entityType)
		{
			return ValidateEntityPermissionForCurrentUser(entityType);
		}

		public IPermissionResult ValidateEntityPermissionForUser(Type entityType, int userId)
		{
			return new PermissionResult(permissionValidator.Validate(entityType, userId));
		}

		public bool ValidatePresetPermissionForCurrentUser(string permissionName)
		{
			return ValidatePresetPermissionForUser(permissionName, userService.CurrentUserId);
		}

		public bool ValidatePresetPermissionForUser(string permissionName, int userId)
		{
			return presetPermissionValidator.Validate(permissionName, userId);
		}
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