using System;
using System.Reflection;

namespace GammaBinding
{
	public class BindingBridge
	{
		public PropertyInfo SourcePropertyInfo { get; private set;}
		public PropertyInfo TargetPropertyInfo { get; private set;}

		public string SourcePropertyName{
			get { return SourcePropertyInfo.Name;
			}
		}

		public string TargetPropertyName{
			get { return TargetPropertyInfo.Name;
			}
		}

		public BindingBridge (PropertyInfo sourcePropery, PropertyInfo targetPropery)
		{
			SourcePropertyInfo = sourcePropery;
			TargetPropertyInfo = targetPropery;
		}
	}
}

