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

		public PropertyInfo[] TargetPropertyChain { get; private set;}

		public IBindingSource MyBindingSource { get; private set;}

		public BridgeMode Mode { get; private set;}

		public string TargetPropertyName{
			get { return String.Join (".", TargetPropertyChain.Select (p => p.Name));
			}
		}

		public FuncBindingBridge (IBindingSource source, Expression<Func<TSource, object>> sourceGetter, PropertyInfo[] targetProperyChain)
		{
			MyBindingSource = source;
			getter = sourceGetter.Compile ();
			readExpression (sourceGetter);
			TargetPropertyChain = targetProperyChain;
			bool fromSource = true && TargetPropertyChain.Last ().CanWrite;
			bool fromTarget = false && TargetPropertyChain.Last ().CanRead 
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
				MyBindingSource.Controler.TargetSetValue (TargetPropertyChain, GetValueFromSource (source));
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

