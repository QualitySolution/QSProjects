using System;
using System.Collections.Generic;

namespace Gamma.Binding.Core
{
	public interface IBindingSource
	{
		object DataSourceObject { get; }

		IEnumerable<IBindingBridge> BindedBridges { get; }
	}
}
