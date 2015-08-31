using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using Gamma.Binding.Core.Helpers;

namespace Gamma.Binding.Core
{
	public class FuncBindingBridge<TSource> : IBindingBridge
	{
		private string[] SourceProperies;

		private Func<TSource, object> getter;

		public PropertyInfo TargetPropertyInfo { get; private set;}

		public IBindingSource MyBindingSource { get; private set;}

		public BridgeMode Mode { get; private set;}

		public string TargetPropertyName{
			get { return TargetPropertyInfo.Name;
			}
		}

		public FuncBindingBridge (IBindingSource source, Expression<Func<TSource, object>> sourceGetter, PropertyInfo targetPropery)
		{
			MyBindingSource = source;
			getter = sourceGetter.Compile ();
			readExpression (sourceGetter);
			TargetPropertyInfo = targetPropery;
			bool fromSource = true && TargetPropertyInfo.CanWrite;
			bool fromTarget = false && TargetPropertyInfo.CanRead 
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

		public void SourcePropertyUpdated (string propertyName, object source)
		{
			if(SourceProperies.Contains (propertyName))
				MyBindingSource.Controler.TargetSetValue (TargetPropertyInfo, GetValueFromSource (source));
		}

		public object GetValueFromSource (object sourceObject)
		{
			if(sourceObject is TSource)
				return getter ((TSource)sourceObject);
			else
				throw new InvalidCastException (String.Format ("sourceObject should be {0} type.", typeof(TSource)));
		}

		public bool SetValueToSource (object sourceObject, object value)
		{
			throw new NotImplementedException ();
		}

		void readExpression(Expression<Func<TSource, object>> getterFunc)
		{
			SourceProperies = FetchPropertyFromExpression.Fetch (getterFunc);
		}
	}
}

