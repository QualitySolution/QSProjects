using System;
using QS.Services;

namespace QS.Project.Services
{
	public class CurrentPermissionServiceAdapter : ICurrentPermissionService
	{
		readonly IPermissionService PermissionService;
		readonly IUserService UserService;

		public CurrentPermissionServiceAdapter(IPermissionService permissionService, IUserService userService)
		{
			this.PermissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			this.UserService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public IPermissionResult ValidateEntityPermission(Type entityType)
		{
			return PermissionService.ValidateUserPermission(entityType, UserService.CurrentUserId);
		}

		public bool ValidatePresetPermission(string permissionName)
		{
			return PermissionService.ValidateUserPresetPermission(permissionName, UserService.CurrentUserId);
		}
	}
}
