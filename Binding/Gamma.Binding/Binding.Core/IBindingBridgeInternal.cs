using System;
using System.Reflection;

namespace Gamma.Binding.Core
{
	internal interface IBindingBridgeInternal 
	{
		BridgeMode Mode { get;}

		PropertyInfo[] TargetPropertyChain { get;}

		string TargetPropertyName { get;}

		void SourcePropertyUpdated(string propertyName, object sourceObject);

		object GetValueFromSource(object sourceObject);

		bool SetValueToSource(object sourceObject, object value);
	}

	public interface IBindingBridge
	{

	}

	public interface IPropertyBindingBridge : IBindingBridge
	{
		string SourcePropertyName { get; }
	}

	public enum BridgeMode
	{
		OnlyFromSource,
		BackwardFromTarget,
		TwoWay
	}
}

