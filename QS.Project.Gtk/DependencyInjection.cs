using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;

namespace QS.Project.GtkSharp {
	public static class DependencyInjection 
	{
		public static IServiceCollection AddDesktop(this IServiceCollection services) {
			services
				.AddSingleton<IGuiDispatcher, GtkGuiDispatcher>()
				;
			return services;
		}

		/// <summary>
		/// Метод регистрирует зависимости UoW с трекером изменений для Gui
		/// В этом случает трекер будет регистрировать изменения из любого потока 
		/// в UI потоке, что подойдет для GUI приложений
		/// <para><b>ВАЖНО: Взаимоисключающий с другими методами Add*UoW(services)</b></para>
		/// </summary>
		public static IServiceCollection AddGuiTrackedUoW(this IServiceCollection services) {
			services
				.AddSingleton<ITrackerActionInvoker, GuiTrackerInvoker>()
				.AddSingleton<GlobalUowEventsTracker>()
				.AddTransient<SingleUowEventsTracker>()
				.AddSingleton<IUnitOfWorkFactory, TrackedUnitOfWorkFactory>()
				;
			return services;
		}
	}
}
