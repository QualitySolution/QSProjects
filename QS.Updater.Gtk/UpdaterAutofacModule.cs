using System;
using Autofac;
using QS.Project.Versioning;

namespace QS.Updater
{
	public class UpdaterAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<VersionCheckerService>().AsSelf();
			builder.RegisterType<CheckBaseVersion>().AsSelf();
		}
	}
}
