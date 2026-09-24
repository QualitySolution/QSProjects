using Autofac;
using Autofac.Core;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QS.Navigation;

internal static class AvaloniaPageBuilder {
	private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
		var resolveTimer = Stopwatch.StartNew();
		var viewModel = scope.Resolve<TViewModel>(ctorArgs);
		logger.Debug($"Avalonia page: {typeof(TViewModel).Name} разрешена из DI за {resolveTimer.Elapsed.TotalMilliseconds:F0} мс.");
		configureViewModel?.Invoke(viewModel);

		var page = makePage(viewModel, hash);
		page.PageClosed += (_, _) => scope.Dispose();
		return page;
	}
}
