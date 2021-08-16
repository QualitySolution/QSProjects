using System;
using QS.Services;
using QS.Permissions;
using QS.Project.Services.Interactive;
using QS.Validation;
using QS.Dialog;

namespace QS.Project.Services
{
	public static class ServicesConfig
	{
		public static ICommonServices CommonServices => new CommonServices(
			ValidationService,
			InteractiveService,
			PermissionsSettings.PermissionService, 
			PermissionsSettings.CurrentPermissionService,
			UserService);
		
		public static IValidator ValidationService { get; set; }
		public static IInteractiveService InteractiveService { get; set; }

		private static IUserService userService;
		public static IUserService UserService {
			get => userService ?? (userService = new UserService());
			set => userService = value;
		}

		static ServicesConfig()
		{
			ValidationService = new ObjectValidator();
			InteractiveService = new ConsoleInteractiveService();
		}
	}
}
