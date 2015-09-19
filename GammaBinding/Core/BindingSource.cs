using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;
using Gamma.Binding.Core.Helpers;

namespace Gamma.Binding.Core
{
	public class BindingSource<TSource, TTarget> : IBindingSourceInternal
		where TSource : class, INotifyPropertyChanged
	{
		BindingControler<TTarget> myControler;
		readonly List<IBindingBridgeInternal> Bridges = new List<IBindingBridgeInternal> ();

		TSource dataSource;

		public TSource DataSource {
			get {
				return dataSource;
			}
			set {if (dataSource == value)
					return;
				if (dataSource != null)
					dataSource.PropertyChanged -= DataSource_PropertyChanged;
				dataSource = value;
				if(dataSource != null)
					dataSource.PropertyChanged += DataSource_PropertyChanged;
			}
		}

		public object DataSourceObject {
			get { return DataSource;}
		}

		public BindingSource (BindingControler<TTarget> controler, TSource source)
		{
			myControler = controler;
			dataSource = source;
		}

		void DataSource_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			foreach(var bridge in Bridges.FindAll (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				bridge.SourcePropertyUpdated (e.PropertyName, sender);
			}
		}

		#region internal

		IBindingBridgeInternal[] IBindingSourceInternal.AllBridges {
			get {
				return Bridges.ToArray ();
			}
		}

		IBindingControlerInternal IBindingSourceInternal.Controler {
			get {
				return myControler;
			}
		}

		internal IBindingControlerInternal Controler {
			get {
				return myControler;
			}
		}

		IBindingBridgeInternal[] IBindingSourceInternal.GetBackwardBridges(string targetPropName)
		{
			return Bridges.Where (b => b.TargetPropertyName == targetPropName).ToArray ();
		}

		object IBindingSourceInternal.GetValueFromSource(IBindingBridgeInternal bridge)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");
			return bridge.GetValueFromSource (DataSource);
		}

		bool IBindingSourceInternal.SetValueToSource(IBindingBridgeInternal bridge, object value)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");

			return bridge.SetValueToSource (DataSource, value);
		}

		#endregion

		public void InitializeFromSource()
		{
			foreach(var bridge in Bridges.Where (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				myControler.TargetSetValue (bridge.TargetPropertyChain, bridge.GetValueFromSource(DataSource));
			}
		}

		#region config

		public BindingSource<TNewSource, TTarget> AddSource<TNewSource>(TNewSource source)
			where TNewSource : class, INotifyPropertyChanged
		{
			return myControler.AddSource (source);
		}

		public BindingSource<TNewSource, TTarget> AddBinding<TNewSource>(TNewSource source, Expression<Func<TNewSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty)
			where TNewSource : class, INotifyPropertyChanged
		{
			return myControler.AddBinding (source, sourceProperty, targetProperty);
		}

		public BindingSource<TSource, TTarget> AddBinding(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo sourceInfo = PropertyUtil.GetPropertyInfo (sourceProperty);
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo));

			return this;
		}

		public BindingSource<TSource, TTarget> AddFuncBinding(Expression<Func<TSource, object>> sourceGetter, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new FuncBindingBridge<TSource>(this, sourceGetter, targetInfo));

			return this;
		}

		#endregion
	}
}

