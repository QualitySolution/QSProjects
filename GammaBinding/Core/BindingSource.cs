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

		readonly List<IBindingBridge> Bridges = new List<IBindingBridge> ();

		public IBindingBridge[] AllBridges {
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
			foreach(var bridge in Bridges.FindAll (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				bridge.SourcePropertyUpdated (e.PropertyName, sender);
			}
		}

		public IBindingBridge[] GetBackwardBridges(string targetPropName)
		{
			return Bridges.Where (b => b.TargetPropertyName == targetPropName).ToArray ();
		}

		public object GetValueFromSource(IBindingBridge bridge)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");
			return bridge.GetValueFromSource (DataSource);
		}

		public bool SetValueToSource(IBindingBridge bridge, object value)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");

			return bridge.SetValueToSource (DataSource, value);
		}

		public void InitializeFromSource()
		{
			foreach(var bridge in Bridges.Where (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				myControler.TargetSetValue (bridge.TargetPropertyInfo, bridge.GetValueFromSource(DataSource));
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

		public BindingSource<TSource, TTarget> AddFuncBinding(Expression<Func<TSource, object>> sourceGetter, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo targetInfo = PropertyUtil.GetMemberInfo (targetProperty) as PropertyInfo;
			Bridges.Add (new FuncBindingBridge<TSource>(this, sourceGetter, targetInfo));

			return this;
		}

		#endregion
	}
}

