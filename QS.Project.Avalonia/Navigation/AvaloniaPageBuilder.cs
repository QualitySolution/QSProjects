using Autofac;
using Autofac.Core;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace QS.Navigation;

internal static class AvaloniaPageBuilder {
	public static IPage<TViewModel> Create<TViewModel>(
		ILifetimeScope container,
		IEnumerable<Parameter> ctorArgs,
		string hash,
		Action<ContainerBuilder>? addingRegistrations,
		Action<TViewModel>? configureViewModel,
		Func<TViewModel, string, IPage<TViewModel>> makePage)
			where TViewModel : IDialogViewModel
	{
		// Всё, что создано в скоупе страницы, умирает вместе с ней
		var scope = addingRegistrations == null
			? container.BeginLifetimeScope()
			: container.BeginLifetimeScope(addingRegistrations);
		var viewModel = scope.Resolve<TViewModel>(ctorArgs);
		configureViewModel?.Invoke(viewModel);

		var page = makePage(viewModel, hash);
		page.PageClosed += (_, _) => scope.Dispose();
		return page;
	}
}
