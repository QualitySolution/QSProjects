using Autofac;
using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Services;
using QS.Project.Services.Interactive;
using QS.Validation;
using System;

namespace QS.Project {
	public static class DependencyInjections {

		public static IServiceCollection AddConsoleInteracive(this IServiceCollection services) {
			services
				.AddSingleton<IInteractiveMessage, ConsoleInteractiveMessage>()
				.AddSingleton<IInteractiveQuestion, ConsoleInteractiveQuestion>()
				.AddSingleton<IInteractiveService, ConsoleInteractiveService>()
				;
			return services;
		}

		public static IServiceCollection AddObjectValidator(this IServiceCollection services) {
			services.AddSingleton<IValidator>(sp => {
				var validator = new ObjectValidator();
				validator.ServiceProvider = sp;
				return validator;
			});
			return services;
		}

		[Obsolete("Использовать только не в Core проектах, и пока есть необходимость в статике")]
		public static IServiceCollection AddStaticServicesConfig(this IServiceCollection services) {
			services.AddSingleton<OnDatabaseInitialization>((provider) => {
				var scope = provider.GetRequiredService<ILifetimeScope>();
				OrmConfig.Scope = scope;
				ServicesConfig.Scope = scope;
				return new OnDatabaseInitialization();
			});
			return services;
		}
	}
}
