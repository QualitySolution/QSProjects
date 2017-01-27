using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Binding.Core.Helpers;

namespace Gamma.Binding.Core
{
	public class BindingControler<TWidget> : IBindingControlerInternal
	{
		TWidget widget;
		string[] backwardProperties = new string[0];
		List<PropertyInfo[]> delayFiredChains = new List<PropertyInfo[]> ();

		public string[] BackwardProperties {
			get {
				return backwardProperties;
			}
			private set {
				backwardProperties = value;
			}
		}

		List<IBindingSourceInternal> Sources = new List<IBindingSourceInternal> ();

		public BindingControler (TWidget targetWidget, string[] backwards) : this(targetWidget)
		{
			BackwardProperties = backwards;
		}

		public BindingControler (TWidget targetWidget, Expression<Func<TWidget, object>>[] backwardsExp) : this(targetWidget)
		{
			BackwardProperties = backwardsExp.Select (exp => 
				String.Join (".", PropertyChainFromExp.Get (exp).Select (p => p.Name)))
				.ToArray ();
		}

		public BindingControler (TWidget targetWidget)
		{
			widget = targetWidget;
		}

		#region internal

		internal bool IsSourceBatchUpdate { get; set;}

		internal void FinishSourceUpdateBatch()
		{
			IsSourceBatchUpdate = false;
			Sources.ForEach (s => s.RunDelayedUpdates ());
		}

		internal bool IsTargetBatchUpdate { get; set;}

		internal void FinishTargetUpdateBatch()
		{
			IsTargetBatchUpdate = false;
			delayFiredChains.ForEach (
				c => SourceSetValue (PropertyChainFromExp.GetChainName (c), TargetGetValue (c)));
		}

		void IBindingControlerInternal.TargetSetValue(PropertyInfo[] propertyChain, object value)
		{
			this.TargetSetValue(propertyChain, value);
		}

		internal void TargetSetValue(PropertyInfo[] propertyChain, object value)
		{
			object target = widget;
			PropertyInfo lastProp = null;
			foreach(PropertyInfo curProp in propertyChain)
			{
				if (lastProp != null)
					target = lastProp.GetValue (target, null);
				lastProp = curProp;
			}

			object currentValue = lastProp.GetValue (target, null);
			if((currentValue == null && value != null) || (currentValue != null && !currentValue.Equals (value)))
				lastProp.SetValue (target, value, null);
		}

		internal object TargetGetValue(PropertyInfo[] propertyChain)
		{
			object target = widget;
			PropertyInfo lastProp = null;
			foreach(PropertyInfo curProp in propertyChain)
			{
				if (lastProp != null)
					target = lastProp.GetValue (target, null);
				lastProp = curProp;
			}
			return lastProp.GetValue (target, null);
		}

		internal bool SourceSetValue(string property, object value)
		{
			bool anyseted = false;
			if (!BackwardProperties.Contains (property))
				throw new InvalidOperationException (String.Format ("Property {0} is not set as backward.", property));
			foreach(var source in Sources)
			{
				foreach(var bridge in source.GetBackwardBridges (property))
				{
					if (source.SetValueToSource (bridge, value))
						anyseted = true;
				}
			}
			return anyseted;
		}

		#endregion

		public void FireChange(params Expression<Func<TWidget, object>>[] targetProperties)
		{
			bool needSetSourceAsBatch = !IsTargetBatchUpdate && targetProperties.Length > 1;
			if (needSetSourceAsBatch)
				IsSourceBatchUpdate = true;
			foreach (var Property in targetProperties) {
				var chain = PropertyChainFromExp.Get (Property);
				if (IsTargetBatchUpdate)
				{
					if(!delayFiredChains.Exists (c => PropertyChainFromExp.GetChainName (chain) == PropertyChainFromExp.GetChainName (c)))
						delayFiredChains.Add (chain);
				}
				else
				{
					SourceSetValue (
						PropertyChainFromExp.GetChainName (chain),
						TargetGetValue (chain)
					);
				}
			}
			if (needSetSourceAsBatch)
				FinishSourceUpdateBatch ();
		}

		public BindingSource<TSource, TWidget> AddBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingObjectSource<TWidget> AddBinding(object source, string sourcePropertyName, Expression<Func<TWidget, object>> targetProperty)
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourcePropertyName, targetProperty);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty, IValueConverter converter)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourceProperty, targetProperty, converter);
			return sourceControl;
		}

		public BindingObjectSource<TWidget> AddBinding(object source, string sourcePropertyName, Expression<Func<TWidget, object>> targetProperty, IValueConverter converter)
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourcePropertyName, targetProperty, converter);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddFuncBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddFuncBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingObjectSource<TWidget> AddFuncBinding(object source, Expression<Func<object, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
		{
			var sourceControl = AddSource (source);
			sourceControl.AddFuncBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddSource<TSource>(TSource source)
			where TSource : class, INotifyPropertyChanged
		{
			BindingSource<TSource, TWidget> bSource = Sources.Find (s => s.DataSourceObject == source) as BindingSource<TSource, TWidget>;
			if (bSource == null) {
				bSource = new BindingSource<TSource, TWidget> (this, source);
				Sources.Add (bSource);
			}
			return bSource;
		}

		public BindingObjectSource<TWidget> AddSource(object source)
		{
			BindingObjectSource<TWidget> bSource = Sources.OfType<BindingObjectSource<TWidget>> ().FirstOrDefault (s => s.DataSourceObject == source);
			if (bSource == null) {
				bSource = new BindingObjectSource<TWidget> (this, source);
				Sources.Add (bSource);
			}
			return bSource;
		}

		public void InitializeFromSource()
		{
			IsTargetBatchUpdate = true;
			Sources.ForEach (s => s.RunInitializeFromSource());
			FinishTargetUpdateBatch ();
		}

		public void RefreshFromSource()
		{
			IsTargetBatchUpdate = true;
			Sources.ForEach (s => s.RunInitializeFromSource());
			FinishTargetUpdateBatch ();
		}

		public void CleanSources()
		{
			Sources.ForEach(x => x.ClearBindings());

			Sources.Clear();
		}
	}
}

