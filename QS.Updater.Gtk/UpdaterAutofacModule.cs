using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Autofac;
using QS.Project.Versioning;
using QS.Updater.DB;
using QS.Updater.DB.ViewModels;
using QS.ViewModels;

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
			builder.RegisterType<SQLDBUpdater>().As<IDBUpdater>();
			//GTK UI
			builder.RegisterType<UpdaterGtkUI>().As<IUpdaterUI>();
			#region ViewModels
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(UpdateProcessViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion

		}
	}
}
