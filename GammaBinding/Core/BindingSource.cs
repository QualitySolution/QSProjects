using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;

namespace Gamma.Binding.Core
{
	public class BindingSource<TSource, TTarget> : IBindingSource
		where TSource : class, INotifyPropertyChanged
	{
		BindingControler<TTarget> myControler;

		public IBindingControler Controler {
			get {
				return myControler;
			}
		}

		readonly List<BindingBridge> Bridges = new List<BindingBridge> ();

		public BindingBridge[] AllBridges {
			get {
				return Bridges.ToArray ();
			}
		}

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

		public BindingSource (BindingControler<TTarget> controler, TSource source)
		{
			myControler = controler;
			dataSource = source;
		}

		void DataSource_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			foreach(var bridge in Bridges.FindAll (b => b.SourcePropertyName == e.PropertyName && b.Mode != BridgeMode.BackwardFromTarget))
			{
				myControler.TargetSetValue (bridge.TargetPropertyInfo, bridge.SourcePropertyInfo.GetValue (sender, null));
			}
		}

		public BindingBridge[] GetBackwardBridges(string targetPropName)
		{
			return Bridges.Where (b => b.TargetPropertyName == targetPropName).ToArray ();
		}

		public object GetValueFromSource(BindingBridge bridge)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");
			return bridge.SourcePropertyInfo.GetValue (DataSource, null);
		}

		public bool SetValueToSource(BindingBridge bridge, object value)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");
			if(bridge.SourcePropertyInfo.GetValue (DataSource, null) != value)
			{
				bridge.SourcePropertyInfo.SetValue (DataSource, value, null);
				return true;
			}
			return false;
		}

		public void InitializeFromSource()
		{
			foreach(var bridge in Bridges.Where (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				myControler.TargetSetValue (bridge.TargetPropertyInfo, bridge.SourcePropertyInfo.GetValue (DataSource, null));
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
			PropertyInfo sourceInfo = PropertyUtil.GetMemberInfo (sourceProperty) as PropertyInfo;
			PropertyInfo targetInfo = PropertyUtil.GetMemberInfo (targetProperty) as PropertyInfo;
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo));

			return this;
		}

		#endregion
	}
}

