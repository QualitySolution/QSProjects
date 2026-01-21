using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.Project.Dialog;
using QS.Project.Interactive;

namespace QS.Project;

public static partial class DependencyInjection {
	/// <summary>
	/// Настраиваем интерфейс IInteractive... на Avalonia
	/// </summary>
	public static IServiceCollection AddInteractive(this IServiceCollection services) {
		return services
			.AddSingleton<IInteractiveMessage, AvaloniaInteractiveMessage>()
			.AddSingleton<IInteractiveQuestion, AvaloniaInteractiveQuestion>()
			.AddSingleton<IInteractiveService, AvaloniaInteractiveService>();
	}

	public static IServiceCollection AddGuiClasses(this IServiceCollection services) {
		return services
			.AddSingleton<IGuiDispatcher, AvaloniaGuiDispatcher>();
	}
}
