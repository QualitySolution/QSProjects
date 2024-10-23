using Autofac;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Navigation;

public class AvaloniaPageTabFactory(ILifetimeScope container) : IViewModelsPageFactory {
	public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(
		DialogViewModelBase master,
		IDictionary<string, object> ctorArgs,
		string hash,
		Action<ContainerBuilder> addingRegistrations,
		Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase
	{
		var scope = addingRegistrations == null ? container.BeginLifetimeScope() : container.BeginLifetimeScope(addingRegistrations);
		var viewmodel = scope.Resolve<TViewModel>(ctorArgs.Select(pair => new NamedParameter(pair.Key, pair.Value)));
		configureViewModel?.Invoke(viewmodel);

		var page = new AvaloniaPage<TViewModel>(viewmodel, hash);
		page.PageClosed += (sender, e) => scope.Dispose();
		return page;
	}

	public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(
		DialogViewModelBase master,
		Type[] ctorTypes,
		object[] ctorValues,
		string hash,
		Action<ContainerBuilder> addingRegistrations,
		Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase
	{
		var scope = addingRegistrations == null ? container.BeginLifetimeScope() : container.BeginLifetimeScope(addingRegistrations);
		var viewmodel = scope.Resolve<TViewModel>(ctorTypes.Zip(ctorValues, (type, val) => new TypedParameter(type, val)));
		configureViewModel?.Invoke(viewmodel);

		var page = new AvaloniaPage<TViewModel>(viewmodel, hash);
		page.PageClosed += (sender, e) => scope.Dispose();
		return page;
	}
}
