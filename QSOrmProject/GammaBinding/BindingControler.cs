using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace GammaBinding
{
	public class BindingControler<TWidget>
	{
		TWidget widget;

		public BindingControler (TWidget targetWidget)
		{
			widget = targetWidget;
		}

		internal void TargetSetValue(PropertyInfo property, object value)
		{
			property.SetValue (widget, value, null);
		}

		public BindingSource<TSource, TWidget> AddBinding<TSource>(TSource source, Expression<Func<TSource, object>> sourceProperty, Expression<Func<TWidget, object>> targetProperty)
			where TSource : class, INotifyPropertyChanged
		{
			var sourceControl = AddSource (source);
			sourceControl.AddBinding (sourceProperty, targetProperty);
			return sourceControl;
		}

		public BindingSource<TSource, TWidget> AddSource<TSource>(TSource source)
			where TSource : class, INotifyPropertyChanged
		{
			return new BindingSource<TSource, TWidget> (this, source);
		}
	}
}

