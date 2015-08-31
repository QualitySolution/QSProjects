using System;

namespace Gamma.Binding.Core
{
	public interface IBindingSource
	{
		IBindingBridge[] AllBridges { get;}
		IBindingControler Controler{ get;}

		IBindingBridge[] GetBackwardBridges(string targetPropName);
		object GetValueFromSource(IBindingBridge bridge);
		bool SetValueToSource(IBindingBridge bridge, object value);
		void InitializeFromSource();
	}
}

