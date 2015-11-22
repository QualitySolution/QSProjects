using System;
using System.Linq;
using System.Reflection;
using System.Globalization;

namespace Gamma.Binding.Core
{
	internal class BindingBridge : IBindingBridgeInternal
	{
		public PropertyInfo SourcePropertyInfo { get; private set;}
		public PropertyInfo[] TargetPropertyChain { get; private set;}
		public IValueConverter ValueConverter { get; private set;}
		public object ConverterParameter { get; private set;}
		public CultureInfo ConverterCulture { get; private set;}

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

		internal BindingBridge (IBindingSourceInternal source, PropertyInfo sourcePropery, PropertyInfo[] targetProperyChain, IValueConverter converter)
			: this(source, sourcePropery, targetProperyChain)
		{
			ValueConverter = converter;
		}

		internal BindingBridge (IBindingSourceInternal source, PropertyInfo sourcePropery, PropertyInfo[] targetProperyChain, IValueConverter converter, object converterParameter, CultureInfo converterCulture)
			: this(source, sourcePropery, targetProperyChain, converter)
		{
			ConverterParameter = converterParameter;
			ConverterCulture = converterCulture;
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
			{
				MyBindingSource.Controler.TargetSetValue (TargetPropertyChain, (this as IBindingBridgeInternal).GetValueFromSource (source));
			}
		}

		object IBindingBridgeInternal.GetValueFromSource (object sourceObject)
		{
			if(ValueConverter != null)
				return ValueConverter.Convert (
					SourcePropertyInfo.GetValue (sourceObject, null),
					TargetPropertyChain.Last ().PropertyType,
					ConverterParameter,
					ConverterCulture ?? CultureInfo.CurrentUICulture
					);
			else
				return SourcePropertyInfo.GetValue (sourceObject, null);
		}

		bool IBindingBridgeInternal.SetValueToSource (object sourceObject, object value)
		{
			object prepared = ValueConverter == null ? value
				: ValueConverter.ConvertBack ( value,
					SourcePropertyInfo.PropertyType,
					ConverterParameter,
					ConverterCulture ?? CultureInfo.CurrentUICulture
				);

			if(SourcePropertyInfo.GetValue (sourceObject, null) != prepared)
			{
				SourcePropertyInfo.SetValue (sourceObject, prepared, null);
				return true;
			}
			return false;
		}
	}
}

