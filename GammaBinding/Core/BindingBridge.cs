using System;
using System.Linq;
using System.Reflection;

namespace Gamma.Binding.Core
{
	public class BindingBridge
	{
		public PropertyInfo SourcePropertyInfo { get; private set;}
		public PropertyInfo TargetPropertyInfo { get; private set;}

		public IBindingSource MyBindingSource { get; private set;}

		public BridgeMode Mode { get; private set;}

		public string SourcePropertyName{
			get { return SourcePropertyInfo.Name;
			}
		}

		public string TargetPropertyName{
			get { return TargetPropertyInfo.Name;
			}
		}

		public BindingBridge (IBindingSource source, PropertyInfo sourcePropery, PropertyInfo targetPropery)
		{
			MyBindingSource = source;
			SourcePropertyInfo = sourcePropery;
			TargetPropertyInfo = targetPropery;
			bool fromSource = SourcePropertyInfo.CanRead && TargetPropertyInfo.CanWrite;
			bool fromTarget = SourcePropertyInfo.CanWrite && TargetPropertyInfo.CanRead 
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
	}

	public enum BridgeMode
	{
		OnlyFromSource,
		BackwardFromTarget,
		TwoWay
	}
}

