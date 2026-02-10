using Autofac;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace QS.Navigation;

public class AvaloniaPageWindowFactory : IViewModelsPageFactory {
	public IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(IDialogViewModel master, IDictionary<string, object> ctorArgs, string hash, Action<ContainerBuilder> addingRegistrations, Action<TViewModel> configureViewModel = null) where TViewModel : IDialogViewModel {
		throw new NotImplementedException();
	}

	public IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(IDialogViewModel master, Type[] ctorTypes, object[] ctorValues, string hash, Action<ContainerBuilder> addingRegistrations, Action<TViewModel> configureViewModel = null) where TViewModel : IDialogViewModel {
		throw new NotImplementedException();
	}
}
