using System;
using System.Reflection;

namespace Gamma.Binding.Core
{
	public interface IBindingBridge
	{
		BridgeMode Mode { get;}

		PropertyInfo TargetPropertyInfo { get;}

		string TargetPropertyName { get;}

		void SourcePropertyUpdated(string propertyName, object sourceObject);

		object GetValueFromSource(object sourceObject);

		bool SetValueToSource(object sourceObject, object value);
	}

	public enum BridgeMode
	{
		OnlyFromSource,
		BackwardFromTarget,
		TwoWay
	}
}

