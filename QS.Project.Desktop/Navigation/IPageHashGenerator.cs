using System;
using System.Collections.Generic;
using QS.ViewModels.Dialog;

namespace QS.Navigation
{
	public interface IPageHashGenerator
	{
		string GetHash<TViewModel>(DialogViewModelBase master, Type[] ctorTypes, object[] ctorValues);
		string GetHashNamedArgs<TViewModel>(DialogViewModelBase master, IDictionary<string, object> ctorArgs);
	}
}
