using System;
using System.Reflection;

namespace Gamma.Binding.Core
{
	public interface IBindingControler
	{
		string[] BackwardProperties { get;}

		void TargetSetValue(PropertyInfo property, object value);
	}
}

