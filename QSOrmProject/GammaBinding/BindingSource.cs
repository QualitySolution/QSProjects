using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using QSOrmProject;

namespace GammaBinding
{
	public class BindingSource<TSource, TTarget>  where TSource : class, INotifyPropertyChanged
	{
		BindingControler<TTarget> myControler;

		List<BindingBridge> Bridges = new List<BindingBridge> ();

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

		void DataSource_PropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			foreach(var bridge in Bridges.FindAll (b => b.SourcePropertyName == e.PropertyName))
			{
				myControler.TargetSetValue (bridge.TargetPropertyInfo, bridge.SourcePropertyInfo.GetValue (sender, null));
			}
		}

		public BindingSource (BindingControler<TTarget> controler, TSource source)
		{
			myControler = controler;
			dataSource = source;
		}

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
			Bridges.Add (new BindingBridge(sourceInfo, targetInfo));

			return this;
		}
	}
}

