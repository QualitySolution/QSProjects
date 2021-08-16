using System;
using System.Collections;
using System.Reflection;

namespace Gamma.Binding
{
	public interface IRecursiveTreeConfig
	{
		PropertyInfo ParentProperty { get;}
		PropertyInfo ChildsCollectionProperty { get;}

		IyTreeModel CreateModel(IList list);
	}
}

