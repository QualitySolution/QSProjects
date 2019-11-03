using System;
using System.Collections.Generic;
using QS.ViewModels;

namespace QS.Navigation
{
	public interface IPageHashGenerator
	{
		string GetHash<TViewModel>(ViewModelBase master, Type[] ctorTypes, object[] ctorValues);
		string GetHashNamedArgs<TViewModel>(ViewModelBase master, IDictionary<string, object> ctorArgs);
	}
}
