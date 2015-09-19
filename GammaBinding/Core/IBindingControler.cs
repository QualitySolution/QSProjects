using System;
using System.Reflection;

namespace Gamma.Binding.Core
{
	public interface IBindingControler
	{
		string[] BackwardProperties { get;}
	}

	internal interface IBindingControlerInternal : IBindingControler
	{
		void TargetSetValue(PropertyInfo[] propertyChain, object value);
	}
}

