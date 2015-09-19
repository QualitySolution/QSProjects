using System;
using System.Linq;
using System.Reflection;

namespace Gamma.Binding.Core
{
	internal class BindingBridge : IBindingBridgeInternal
	{
		public PropertyInfo SourcePropertyInfo { get; private set;}
		public PropertyInfo[] TargetPropertyChain { get; private set;}

		internal IBindingSourceInternal MyBindingSource { get; private set;}

		public BridgeMode Mode { get; private set;}

		public string SourcePropertyName{
			get { return SourcePropertyInfo.Name;
			}
		}

		public string TargetPropertyName{
			get { return String.Join (".", TargetPropertyChain.Select (p => p.Name));
			}
		}

		internal BindingBridge (IBindingSourceInternal source, PropertyInfo sourcePropery, PropertyInfo[] targetProperyChain)
		{
			MyBindingSource = source;
			SourcePropertyInfo = sourcePropery;
			TargetPropertyChain = targetProperyChain;
			bool fromSource = SourcePropertyInfo.CanRead && TargetPropertyChain.Last ().CanWrite;
			bool fromTarget = SourcePropertyInfo.CanWrite && TargetPropertyChain.Last ().CanRead 
				&& MyBindingSource.Controler.BackwardProperties.Contains (TargetPropertyName);
			if (fromSource && fromTarget)
				Mode = BridgeMode.TwoWay;
			else if (fromSource)
				Mode = BridgeMode.OnlyFromSource;
			else if (fromTarget)
				Mode = BridgeMode.BackwardFromTarget;
			else
				throw new ArgumentException ("Невозможно вычислить направление биндинга, необходимо что бы хотябы одно свойство имело возможность записи, а другое чтение.");
		}

		void IBindingBridgeInternal.SourcePropertyUpdated (string propertyName, object source)
		{
			if(SourcePropertyName == propertyName)
				MyBindingSource.Controler.TargetSetValue (TargetPropertyChain, SourcePropertyInfo.GetValue (source, null));
		}

		object IBindingBridgeInternal.GetValueFromSource (object sourceObject)
		{
			return SourcePropertyInfo.GetValue (sourceObject, null);
		}

		bool IBindingBridgeInternal.SetValueToSource (object sourceObject, object value)
		{
			if(SourcePropertyInfo.GetValue (sourceObject, null) != value)
			{
				SourcePropertyInfo.SetValue (sourceObject, value, null);
				return true;
			}
			return false;
		}
	}
}

