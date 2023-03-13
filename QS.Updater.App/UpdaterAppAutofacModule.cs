using Autofac;
using QS.Updater.App.ViewModels;
using QS.Updates;

namespace QS.Updater.App
{
	public class UpdaterAppAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ApplicationUpdater>().As<IAppUpdater>();
			builder.RegisterType<SkipVersionStateIniConfig>().As<ISkipVersionState>();
			builder.RegisterType<ReleasesService>().AsSelf();

			#region ViewModels
			builder.RegisterType<NewVersionViewModel>().AsSelf();
			#endregion
		}
	}
}
