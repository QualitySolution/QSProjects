using System;
using QS.Dialog;
using QS.Validation;

namespace QS.Services
{
	public class CommonServices : ICommonServices
	{
		public CommonServices(
			IValidator validationService, 
			IInteractiveService interactiveService,
			IPermissionService permissionService,
			ICurrentPermissionService currentPermissionService,
			IUserService userService
		)
		{
			ValidationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
			InteractiveService = interactiveService;
			PermissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
			CurrentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			UserService = userService ?? throw new ArgumentNullException(nameof(userService));
		}

		public IValidator ValidationService { get; }

		public IInteractiveService InteractiveService { get; }

		public IPermissionService PermissionService { get; }

		public ICurrentPermissionService CurrentPermissionService { get; }

		public IUserService UserService { get; }
	}
}
