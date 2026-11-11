using System.Reflection;
using System.Linq;
using Autofac;
using QS.Dialog;
using QS.Dialog.ViewModels;
using QS.Navigation;

namespace QS.Project;

public static partial class DependencyInjection {
	/// <summary>Регистрирует общую навигацию Avalonia. Приложение задаёт сборки своих представлений.</summary>
	public static ContainerBuilder AddAvaloniaNavigation(this ContainerBuilder builder, params Assembly[] viewAssemblies) {
		builder.RegisterType<DefaultPageHashGenerator>().As<IPageHashGenerator>().SingleInstance();
		builder.RegisterType<AvaloniaNavigationManager>().AsSelf().As<INavigationManager>().SingleInstance();
		builder.RegisterType<AvaloniaPageTabFactory>();
		builder.RegisterType<AvaloniaPageWindowFactory>();
		builder.Register(ctx => new AvaloniaViewResolver(ctx.Resolve<AvaloniaViewFactory>(),
			viewAssemblies.Append(typeof(AvaloniaNavigationManager).Assembly).Distinct().ToArray())).AsSelf().As<IAvaloniaViewResolver>().SingleInstance();
		builder.Register(ctx => {
			var scope = ctx.Resolve<IComponentContext>();
			return new AvaloniaViewFactory(() => scope.Resolve<IAvaloniaViewResolver>());
		}).SingleInstance();
		builder.RegisterType<ModalProgressCreator>();
		builder.RegisterType<ProgressWindowViewModel>();
		return builder;
	}
}
