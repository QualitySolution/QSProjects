using System;
using System.Collections.Generic;
using Autofac;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public interface IViewModelsPageFactory
	{
		IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(
			DialogViewModelBase master,
			Type[] ctorTypes, object[] ctorValues,
			string hash,
			Action<ContainerBuilder> addingRegistrations,
			Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase;

		IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(
			DialogViewModelBase master,
			IDictionary<string, object> ctorArgs,
			string hash,
			Action<ContainerBuilder> addingRegistrations,
			Action<TViewModel> configureViewModel = null) where TViewModel : DialogViewModelBase;
	}
}
