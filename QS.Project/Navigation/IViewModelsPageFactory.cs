using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface IViewModelsPageFactory
	{
		IPage<TViewModel> CreateViewModelTypedArgs<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues, string hash) where TViewModel : DialogViewModelBase;

		IPage<TViewModel> CreateViewModelNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs, string hash) where TViewModel : DialogViewModelBase;
	}
}
