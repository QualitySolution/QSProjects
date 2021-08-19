using System;

namespace Gamma.Binding.Core
{
	internal interface IBindingSourceInternal : IBindingSource
	{
		IBindingBridgeInternal[] AllBridges { get;}
		IBindingControlerInternal Controler{ get;}

		IBindingBridgeInternal[] GetBackwardBridges(string targetPropName);

		object GetValueFromSource(IBindingBridgeInternal bridge);
		bool SetValueToSource(IBindingBridgeInternal bridge, object value);
		void RunInitializeFromSource();
		void RunDelayedUpdates();
		void ClearBindings();
	}
}

