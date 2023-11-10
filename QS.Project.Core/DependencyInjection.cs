using Microsoft.Extensions.DependencyInjection;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;

namespace QS.Project.Core {
	public static class DependencyInjection 
	{
		/// <summary>
		/// Метод регистрации основных зависимостей Core модуля,
		/// которые должны использоваться для всех приложений использующих данный модуль
		/// </summary>
		public static IServiceCollection AddCoreServices(IServiceCollection services) {
			// Тут регистрируем все необходимые зависимости зависимости

			return services;
		}

		// Желательно выделить в отдельную библиотеку
		/// <summary>
		/// Метод регистрации серверных зависимостей определенных в модуле Core
		/// которые должны использоваться для всех серверных приложений использующих данный модуль
		/// </summary>
		public static IServiceCollection AddCoreServerServices(IServiceCollection services) {
			StaticRegistrations.AddServerStaticRegistrations();

			services
				.AddSingleton<IMainThreadDispatcher>(StaticRegistrations.ThreadDispatcher)
				;
			return services;
		}
	}

	[Obsolete("Временное решение пока в зависимом коде не будет удалена статика")]
	public static class StaticRegistrations 
	{
		public static IMainThreadDispatcher ThreadDispatcher { get; set; }

		/// <summary>
		/// Регистрация обязательных статических зависимостей для серверных приложений
		/// </summary>
		public static void AddServerStaticRegistrations() {
			ThreadDispatcher = new ServerThreadDispatcher();
			UnitOfWorkFactory.MainThreadDispatcher = ThreadDispatcher;
		}
	}
}
