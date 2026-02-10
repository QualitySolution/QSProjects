using System;
using System.Collections.Generic;

namespace QS.Navigation
{
	public interface IPageHashGenerator
	{
		string GetHash<TViewModel>(IDialogViewModel master, Type[] ctorTypes, object[] ctorValues);
		string GetHashNamedArgs<TViewModel>(IDialogViewModel master, IDictionary<string, object> ctorArgs);
	}
}
