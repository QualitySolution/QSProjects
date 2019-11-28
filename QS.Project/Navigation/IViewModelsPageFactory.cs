using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface IViewModelsPageFactory
	{
		IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash) where TViewModel : ViewModelBase;

		IPage CreateViewModelTypedArgs(Type viewModelType, ViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash);

		IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs, string hash) where TViewModel : ViewModelBase;

		IPage CreateViewModelNamedArgs(Type viewModelType, ViewModelBase master, IDictionary<string, object> ctorArgs, string hash);
	}
}
