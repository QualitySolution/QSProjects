using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Gamma.Binding.Core
{
	public abstract class BindingSourceBase<TTarget> : IBindingSourceInternal
	{
		BindingControler<TTarget> myControler;
		internal readonly List<IBindingBridgeInternal> Bridges = new List<IBindingBridgeInternal> ();
		private object _locker = new object();
		List<string> delayedUpdateProperties = new List<string>();

		public BindingSourceBase (BindingControler<TTarget> controler)
		{
			myControler = controler;
		}

		#region internal

		public abstract object DataSourceObject { get; set;}

		IBindingBridgeInternal[] IBindingSourceInternal.AllBridges {
			get {
				return Bridges.ToArray ();
			}
		}

		public IEnumerable<IBindingBridge> BindedBridges => Bridges.Cast<IBindingBridge>().AsEnumerable();

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
			return bridge.GetValueFromSource (DataSourceObject);
		}

		bool IBindingSourceInternal.SetValueToSource(IBindingBridgeInternal bridge, object value)
		{
			if (!Bridges.Contains (bridge))
				throw new InvalidOperationException ("Bridge не из этого источника.");

			if(bridge.Mode == BridgeMode.OnlyFromSource)
				return false;

			return bridge.SetValueToSource (DataSourceObject, value);
		}

		void IBindingSourceInternal.RunInitializeFromSource()
		{
			foreach(var bridge in Bridges.Where (b => b.Mode != BridgeMode.BackwardFromTarget))
			{
				myControler.TargetSetValue (bridge.TargetPropertyChain, bridge.GetValueFromSource(DataSourceObject));
			}
		}

		void IBindingSourceInternal.RunDelayedUpdates()
		{
			//Used loop "for" because when PropertyUpdated excuted in delayedUpdateProperties may be adding propery.
			for (int i = 0; i < delayedUpdateProperties.Count; i++)
				PropertyUpdated(delayedUpdateProperties[i]);
			delayedUpdateProperties.Clear ();
		}

		#endregion

		protected void DataSource_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (myControler.IsSourceBatchUpdate) {
				if(!delayedUpdateProperties.Contains (e.PropertyName))
					delayedUpdateProperties.Add (e.PropertyName);
			} else {
				if(myControler.IsThreadSafeBinding) {
					Gtk.Application.Invoke((s, args) => PropertyUpdated(e.PropertyName));
				}
				else {
					PropertyUpdated(e.PropertyName);
				}
			}
		}

		protected void PropertyUpdated(string propertyName)
		{
			if(myControler.IsThreadSafeBinding) {
				lock(_locker) {
					foreach (var bridge in Bridges.FindAll(b => b.Mode != BridgeMode.BackwardFromTarget)) {
						bridge.SourcePropertyUpdated(propertyName, DataSourceObject);
					}
				}
			}
			else {
				foreach (var bridge in Bridges.FindAll(b => b.Mode != BridgeMode.BackwardFromTarget)) {
					bridge.SourcePropertyUpdated(propertyName, DataSourceObject);
				}
			}
		}

		public abstract void ClearBindings();

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

		public BindingSource<TNewSource, TTarget> AddBinding<TNewSource>(TNewSource source, Expression<Func<TNewSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter)
			where TNewSource : class, INotifyPropertyChanged
		{
			return myControler.AddBinding (source, sourceProperty, targetProperty, converter);
		}

		public BindingObjectSource<TTarget> AddSource(object source)
		{
			return myControler.AddSource (source);
		}

		public BindingObjectSource<TTarget> AddBinding(object source, string sourcePropertyName, Expression<Func<TTarget, object>> targetProperty)
		{
			return myControler.AddBinding (source, sourcePropertyName, targetProperty);
		}

		public BindingObjectSource<TTarget> AddBinding(object source, string sourcePropertyName, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter)
		{
			return myControler.AddBinding (source, sourcePropertyName, targetProperty, converter);
		}

		public void InitializeFromSource()
		{
			myControler.InitializeFromSource ();
		}

		#endregion
	}
}

