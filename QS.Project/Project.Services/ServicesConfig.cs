using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Permissions;
using QS.Services;
using QS.Validation;
using System;

namespace QS.Project.Services {
	[Obsolete("Статические регистрации для устаревших проектов. " +
		"Вместо этого нужно использовать получение зависимостей из контейнера зависимостей.")]
	public static class ServicesConfig
	{
		public static ILifetimeScope Scope { get; set; }

		public static ICommonServices CommonServices => new CommonServices(
			ValidationService,
			InteractiveService,
			PermissionsSettings.PermissionService, 
			PermissionsSettings.CurrentPermissionService,
			UserService);
		
		public static IValidator ValidationService => Scope.Resolve<IValidator>();

		//Опционально, потому что в серверных проектах не нужен данный сервис
		public static IInteractiveService InteractiveService => Scope.ResolveOptional<IInteractiveService>();
		public static IUserService UserService => Scope.Resolve<IUserService>();
		public static IUnitOfWorkFactory UnitOfWorkFactory => Scope.Resolve<IUnitOfWorkFactory>();
	}
}
