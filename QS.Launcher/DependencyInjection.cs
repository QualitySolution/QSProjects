using Autofac;
using Microsoft.Extensions.DependencyInjection;
using QS.DBScripts.Controllers;
using QS.DbManagement;
using QS.ErrorReporting;
using QS.ErrorReporting.Handlers;
using QS.Launcher.AppRunner;
using QS.Launcher.Services;
using QS.Launcher.ViewModels;
using QS.Launcher.ViewModels.PageViewModels;
using QS.Launcher.ViewModels.PageViewModels.DataBase;
using QS.DbManagement.Creation;
using System;
using System.Collections.Generic;

namespace QS.Launcher {
	public static partial class DependencyInjection {
		public static IServiceCollection AddLauncherDataBaseCreation(this IServiceCollection services, List<(Type res,Type creator)> resourceCratorMap)
		{
			var map = new DbResourcesCreationMap();
			foreach(var resourceCrator in resourceCratorMap) {
				map.Register(resourceCrator.res, resourceCrator.creator);
			}

			return services
				.AddSingleton(map)
				.AddSingleton<DbCreationFactory>();
		}

		public static IServiceCollection AddLauncherViewModels(this IServiceCollection services) {
			return services
				.AddSingleton<MainWindowVM>()
				.AddSingleton<LoginVM>()
				.AddSingleton<DataBasesVM>()
				.AddSingleton<UsersVM>()
				// Страницы разовой операции создаются заново на каждый вызов: иначе состояние
				// прошлого редактирования приходится вычищать руками, а подписки - накапливаются
				.AddTransient<UserManagementVM>()
				.AddTransient<ChangePasswordVM>()
				.AddTransient<CreateDataBaseProgressVM>()
				.AddSingleton<IDbCreatorInteraction, LauncherDbCreatorInteraction>()
				.AddSingleton<DbCapabilities>();
		}

		public static IServiceCollection AddLauncherOptions(this IServiceCollection services, LauncherOptions launcherOptions) {
			return services.AddSingleton(launcherOptions);
		}
		
		public static IServiceCollection AddLauncherDependencies(this IServiceCollection services) {
			return services
				.AddSingleton<Configurator>();
		}

		/// <summary>
		/// Разбор ошибок лаунчера. Порядок регистрации обработчиков - это и есть порядок разбора:
		/// первый, кто узнал ошибку, забирает её себе. Сначала то, в чём разработчики не виноваты
		/// (нет прав, сервер отказал в доступе), потом сеть, и только остаток идёт в отчёт.
		///
		/// <see cref="IErrorReporter"/> не регистрируем: отправлять ли отчёты и куда - решает
		/// приложение, у библиотеки нет ни кода продукта, ни адреса сервиса.
		/// </summary>
		public static IServiceCollection AddLauncherErrorHandling(this IServiceCollection services) {
			return services
				.AddSingleton<IErrorHandler, NotEnoughRights>()
				.AddSingleton<IErrorHandler, MySqlExceptionLoginFailed>()
				.AddSingleton<IErrorHandler, ConnectionIsLost>()
				.AddSingleton<IErrorHandler, MySqlExceptionAccessDenied>()
				.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
		}

		public static IServiceCollection AddConnectionType(this IServiceCollection services, ConnectionTypeBase connectionType) {
			services.AddSingleton(connectionType);
			return services;
		}

		#region AppRunner

		public static IServiceCollection UseInProcessRunner(this IServiceCollection services) {
			services.AddSingleton<InProcessRunner>();
			services.AddSingleton<IAppRunner>(sp => sp.GetRequiredService<InProcessRunner>());
			return services;
		}
		
		// Для Autofac - регистрируем как интерфейс и класс одновременно
		public static void UseInProcessRunner(this ContainerBuilder builder) {
			builder.RegisterType<InProcessRunner>().As<IAppRunner>().AsSelf().SingleInstance();
		}
		
		public static IServiceCollection UseNewProcessRunner(this IServiceCollection services, string executableFileName) {
			return services.AddSingleton<IAppRunner>(c => new NewProcessRunner(executableFileName));
		}

		#endregion
	}
}
