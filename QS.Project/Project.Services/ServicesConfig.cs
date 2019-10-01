using System;
using QS.Services;
using QS.Permissions;
using QS.Project.Services.Interactive;

namespace QS.Project.Services
{
	public static class ServicesConfig
	{
		public static ICommonServices CommonServices => new CommonServices(ValidationService, InteractiveService, PermissionsSettings.PermissionService, UserService);
		public static IValidationService ValidationService { get; set; }
		public static IInteractiveService InteractiveService { get; set; }
		public static IUserService UserService { get; set; }

		static ServicesConfig()
		{
			ValidationService = new ValidationServiceWithoutView();
			InteractiveService = new ConsoleInteractiveService();
			UserService = new UserService();
		}
	}
}
