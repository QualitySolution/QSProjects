using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Utilities;

namespace Gamma.Binding.Core
{
	public class BindingControler<TWidget> : IBindingControler
	{
		TWidget widget;
		public string[] BackwardProperties { get; private set;}
		List<IBindingSource> Sources = new List<IBindingSource> ();

		public BindingControler (TWidget targetWidget, string[] backwards) : this(targetWidget)
		{
			BackwardProperties = backwards;
		}

		public BindingControler (TWidget targetWidget)
		{
			widget = targetWidget;
		}

		public void TargetSetValue(PropertyInfo property, object value)
		{
			property.SetValue (widget, value, null);
		}

		internal bool SourceSetValue(string property, object value)
		{
			bool anyseted = false;
			if (!BackwardProperties.Contains (property))
				throw new InvalidOperationException (String.Format ("Свойство {0}, не задано в качестве возвращающего значения в источник.", property));
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

		public void FireChange(Expression<Func<TWidget, object>> targetProperty)
		{
			PropertyInfo targetInfo = PropertyUtil.GetMemberInfo (targetProperty) as PropertyInfo;
			SourceSetValue (targetInfo.Name, targetInfo.GetValue (widget, null));
		}

		public BindingSource<TSource, TWidget> AddBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddFuncBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddFuncBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddSource<TSource>(TSource source)
			where TSource : class, INotifyPropertyChanged
		{
			var bSource = new BindingSource<TSource, TWidget> (this, source);
			Sources.Add (bSource);
			return bSource;
		}

		public void InitializeFromSource()
		{
			Sources.ForEach (s => s.InitializeFromSource());
		}
	}
}

