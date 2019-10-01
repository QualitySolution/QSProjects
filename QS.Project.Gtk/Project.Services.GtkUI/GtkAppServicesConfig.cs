using QS.Permissions;
using QS.Services;
using QS.Validation;
using QS.Validation.GtkUI;

namespace QS.Project.Services.GtkUI
{
	public static class GtkAppServicesConfig
	{
		public static void CreateDefaultGtkServices()
		{
			ServicesConfig.InteractiveService = new GtkInteractiveService();
			ServicesConfig.ValidationService = new ValidationService(new GtkValidationViewFactory());
		}
	}
}
