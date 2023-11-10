using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;

namespace QS.Project.GtkSharp {
	public static class DependencyInjection 
	{
		public static IServiceCollection AddDesktopServices(this IServiceCollection services) {
			StaticRegistrations.AddStaticRegistrations();

			services
				.AddSingleton<IMainThreadDispatcher>(StaticRegistrations.GuiThreadDispatcher)
				.AddSingleton<IGuiDispatcher>(StaticRegistrations.GuiThreadDispatcher)
				;
			return services;
		}
	}

	[Obsolete("Временное решение пока в зависимом коде не будет удалена статика")]
	public static class StaticRegistrations 
	{
		public static GtkGuiDispatcher GuiThreadDispatcher = new GtkGuiDispatcher();

		public static void AddStaticRegistrations() {
			QS.Project.Core.StaticRegistrations.ThreadDispatcher = GuiThreadDispatcher;
			UnitOfWorkFactory.MainThreadDispatcher = GuiThreadDispatcher;
		}
	}
}
