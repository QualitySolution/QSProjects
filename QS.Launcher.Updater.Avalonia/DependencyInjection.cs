using Autofac;
using QS.Updater;
using QS.Updater.App;
using QS.Updater.App.ViewModels;
using QS.Updates;

namespace QS.Launcher.Updates;

public static class DependencyInjection {
	/// <summary>Регистрирует модуль обновлений. Настройки, сервис каналов, навигацию и платформенные сервисы предоставляет приложение.</summary>
	public static ContainerBuilder AddLauncherUpdates(this ContainerBuilder builder) {
		builder.RegisterType<ReleasesService>().SingleInstance();
		builder.RegisterType<SkipVersionStateIniConfig>().As<ISkipVersionState>().SingleInstance();
		builder.RegisterType<ApplicationUpdateChecker>().SingleInstance();
		builder.RegisterType<NewVersionViewModel>();
		builder.RegisterType<LauncherUpdateService>().As<ILauncherUpdateService>().SingleInstance();
		return builder;
	}
}
