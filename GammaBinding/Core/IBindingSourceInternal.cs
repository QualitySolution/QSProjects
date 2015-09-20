using System;

namespace Gamma.Binding.Core
{
	internal interface IBindingSourceInternal
	{
		IBindingBridgeInternal[] AllBridges { get;}
		IBindingControlerInternal Controler{ get;}

		IBindingBridgeInternal[] GetBackwardBridges(string targetPropName);
		object DataSourceObject { get;}

		object GetValueFromSource(IBindingBridgeInternal bridge);
		bool SetValueToSource(IBindingBridgeInternal bridge, object value);
		void RunInitializeFromSource();
		void RunDelayedUpdates();
	}
}

