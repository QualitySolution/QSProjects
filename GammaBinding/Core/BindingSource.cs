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
	public class BindingSource<TSource, TTarget> : BindingSourceBase<TTarget>, IBindingSourceInternal
		where TSource : class, INotifyPropertyChanged
	{
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

		public override object DataSourceObject {
			get { return DataSource;}
			set { DataSource = (TSource)value;}
		}

		public BindingSource (BindingControler<TTarget> controler, TSource source) : base (controler)
		{
			DataSource = source;
		}

		#region config

		public BindingSource<TSource, TTarget> AddBinding(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo sourceInfo = PropertyUtil.GetPropertyInfo (sourceProperty);
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo));

			return this;
		}

		public BindingSource<TSource, TTarget> AddBinding(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter)
		{
			PropertyInfo sourceInfo = PropertyUtil.GetPropertyInfo (sourceProperty);
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo, converter));

			return this;
		}

		public BindingSource<TSource, TTarget> AddBinding(Expression<Func<TSource, object>> sourceProperty, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter, object converterParameter, System.Globalization.CultureInfo converterCulture)
		{
			PropertyInfo sourceInfo = PropertyUtil.GetPropertyInfo (sourceProperty);
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo, converter, converterParameter, converterCulture));

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

