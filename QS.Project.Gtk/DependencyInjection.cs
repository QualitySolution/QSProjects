using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Project.Services.GtkUI;
using QS.Validation;

namespace QS.Project.GtkSharp {
	public static class DependencyInjection 
	{
		public static IServiceCollection AddDesktop(this IServiceCollection services) {
			services
				.AddSingleton<IGuiDispatcher, GtkGuiDispatcher>()
				;
			return services;
		}

		public static IServiceCollection AddObjectValidatorWithGui(this IServiceCollection services) {
			services.AddSingleton<IValidator>(sp => {
				var validator = new ObjectValidator(new GtkValidationViewFactory());
				validator.ServiceProvider = sp;
				return validator;
			});
			return services;
		}

		public static IServiceCollection AddGuiInteractive(this IServiceCollection services) {
			services
				.AddSingleton<IInteractiveMessage, GtkMessageDialogsInteractive>()
				.AddSingleton<IInteractiveQuestion, GtkQuestionDialogsInteractive>()
				.AddSingleton<IInteractiveService, GtkInteractiveService>()
				;
			return services;
		}
	}
}
