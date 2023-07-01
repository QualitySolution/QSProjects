using Autofac;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class UpdaterDesktopAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CheckBaseVersion>().AsSelf();
			builder.RegisterType<VersionCheckerService>().AsSelf();
		}
	}
}
