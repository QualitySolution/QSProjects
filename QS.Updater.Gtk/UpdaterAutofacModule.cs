using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Autofac;
using QS.Project.Versioning;
using QS.Updater.DB;

namespace QS.Updater
{
	public class UpdaterAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<VersionCheckerService>().AsSelf();
			builder.RegisterType<CheckBaseVersion>().AsSelf();
			builder.RegisterType<ApplicationUpdater>().AsSelf();
			builder.Register(c => new WebChannelFactory<IUpdateService>(new WebHttpBinding { AllowCookies = true }, UpdateConfiguration.ServiceUrl).CreateChannel())
				.As<IUpdateService>();
			builder.RegisterType<SkipVersionStateIniConfig>().As<ISkipVersionState>();
			//GTK UI
			builder.RegisterType<UpdaterGtkUI>().As<IUpdaterUI>();

		}
	}
}
