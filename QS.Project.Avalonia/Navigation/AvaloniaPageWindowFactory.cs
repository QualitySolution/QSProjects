using Autofac;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace QS.Navigation;

public class AvaloniaPageWindowFactory : IViewModelsPageFactory {
	public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations, Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations, Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase {
		throw new NotImplementedException();
	}
}
