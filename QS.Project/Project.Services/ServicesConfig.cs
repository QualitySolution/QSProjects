using System;
using QS.Services;
using QS.Permissions;
using QS.Project.Services.Interactive;
using QS.Validation;

namespace QS.Project.Services
{
	public static class ServicesConfig
	{
		public static ICommonServices CommonServices => new CommonServices(ValidationService, InteractiveService, PermissionsSettings.PermissionService, PermissionsSettings.CurrentPermissionService, UserService);
		public static IValidator ValidationService { get; set; }
		public static IInteractiveService InteractiveService { get; set; }
		public static IUserService UserService { get; set; }

		static ServicesConfig()
		{
			ValidationService = new ObjectValidator();
			InteractiveService = new ConsoleInteractiveService();
			UserService = new UserService();
		}
	}
}
