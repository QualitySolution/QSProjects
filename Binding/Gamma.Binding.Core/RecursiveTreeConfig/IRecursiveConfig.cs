using System.Reflection;

namespace Gamma.Binding.Core.RecursiveTreeConfig
{
	public interface IRecursiveConfig
	{
		PropertyInfo ParentProperty { get; }
		PropertyInfo ChildsCollectionProperty { get; }
	}
}
