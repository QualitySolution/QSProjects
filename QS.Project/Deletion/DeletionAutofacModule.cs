using System;
using Autofac;
using QS.Deletion.ViewModels;

namespace QS.Deletion
{
	public class DeletionAutofacModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PrepareDeletionViewModel>().AsSelf();
			builder.RegisterType<DeletionViewModel>().AsSelf();
			builder.RegisterType<DeletionProcessViewModel>().AsSelf();
		}
	}
}
