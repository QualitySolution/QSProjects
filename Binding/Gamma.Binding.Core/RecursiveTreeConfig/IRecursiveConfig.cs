using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gamma.Binding.Core.RecursiveTreeConfig
{
	public interface IRecursiveConfig
	{
		PropertyInfo ParentProperty { get; }
		PropertyInfo ChildsCollectionProperty { get; }
	}
}
