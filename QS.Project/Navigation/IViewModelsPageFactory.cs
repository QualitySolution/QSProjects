using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface IViewModelsPageFactory
	{
		IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash) where TViewModel : ViewModelBase;

		IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, string hash) where TViewModel : ViewModelBase;
	}
}
