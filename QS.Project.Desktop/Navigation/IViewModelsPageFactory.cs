using System;
using System.Collections.Generic;
using Autofac;

namespace QS.Navigation
{
	public interface IViewModelsPageFactory
	{
		IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(
			IDialogViewModel master,
			Type[] ctorTypes, object[] ctorValues,
			string hash,
			Action<ContainerBuilder> addingRegistrations,
			Action<TViewModel> configureViewModel = null) where TViewModel : IDialogViewModel;

		IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(
			IDialogViewModel master,
			IDictionary<string, object> ctorArgs,
			string hash,
			Action<ContainerBuilder> addingRegistrations,
			Action<TViewModel> configureViewModel = null) where TViewModel : IDialogViewModel;
	}
}
