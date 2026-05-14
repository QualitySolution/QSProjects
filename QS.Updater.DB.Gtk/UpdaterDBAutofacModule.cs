using Autofac;
using QS.DBScripts;
using QS.DBScripts.Controllers;
using QS.DBScripts.Models;
using QS.Updater.DB;
using QS.Updater.DB.ViewModels;
using QS.ViewModels;

namespace QS.Updater
{
	public class UpdaterDBAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SQLDBUpdater>().As<IDBUpdater>();
			#region Models
			builder.RegisterType<MySqlDbCreateModel>().AsSelf();
			builder.RegisterType<DatabaseCharsetChecker>().AsSelf();
			#endregion
			#region Desktop
			builder.RegisterType<UserCreateDbController>().As<IDBCreator>();
			#endregion
			builder.Register(c => {
				var creation = c.Resolve<CreationScript>();
				UpdateConfiguration updates = null;
				if(c.IsRegistered<UpdateConfiguration>())
					updates = c.Resolve<UpdateConfiguration>();
				return new DbScriptsConfigurationAdapter(creation, updates);
			})
			.As<IDbScriptsConfiguration>()
			.SingleInstance()
			.PreserveExistingDefaults();
			#region ViewModels
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(UpdateProcessViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion
		}
	}
}
