using System;
using Autofac;
using QS.Serial;
using QS.Serial.Encoding;
using QS.Serial.ViewModels;
using QS.ViewModels;

namespace QS.Features
{
	public class FeaturesAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SerialNumberEncoder>().AsSelf();
			builder.RegisterType<SerialNumberService>().As<ISerialNumberService>();
			#region ViewModels
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(SerialNumberViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			#endregion
		}
	}
}
