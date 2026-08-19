using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Gamma.Binding.Core.Helpers;

namespace Gamma.Binding.Core
{
	public class BindingObjectSource<TTarget> : BindingSourceBase<TTarget>, IBindingSourceInternal
	{
		private object _dataSource;

		public override object DataSourceObject {
			get => _dataSource;
			set {
				if (_dataSource == value)
					return;
				if (_dataSource is INotifyPropertyChanged oldValue)
					oldValue.PropertyChanged -= DataSource_PropertyChanged;
				_dataSource = value;
				if(_dataSource is INotifyPropertyChanged newValue)
					newValue.PropertyChanged += DataSource_PropertyChanged;
			}
		}

		public BindingObjectSource (BindingControler<TTarget> controler, object source) : base(controler)
		{
			DataSourceObject = source;
		}

		public override void ClearBindings()
		{
			if(_dataSource is INotifyPropertyChanged value) {
				value.PropertyChanged -= DataSource_PropertyChanged;
			}
			_dataSource = null;
		}

		#region config

		public BindingObjectSource<TTarget> AddBinding(string sourcePropertyName, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo sourceInfo = DataSourceObject.GetType ().GetProperty (sourcePropertyName);
			if (sourceInfo == null)
				throw new ArgumentException (String.Format ("Property {0} not found.", sourcePropertyName));
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo));

			return this;
		}

		public BindingObjectSource<TTarget> AddBinding(string sourcePropertyName, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter)
		{
			PropertyInfo sourceInfo = DataSourceObject.GetType ().GetProperty (sourcePropertyName);
			if (sourceInfo == null)
				throw new ArgumentException (String.Format ("Property {0} not found.", sourcePropertyName));

			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo, converter));

			return this;
		}

		public BindingObjectSource<TTarget> AddBinding(string sourcePropertyName, Expression<Func<TTarget, object>> targetProperty, IValueConverter converter, object converterParameter, System.Globalization.CultureInfo converterCulture)
		{
			PropertyInfo sourceInfo = DataSourceObject.GetType ().GetProperty (sourcePropertyName);
			if (sourceInfo == null)
				throw new ArgumentException (String.Format ("Property {0} not found.", sourcePropertyName));

			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new BindingBridge(this, sourceInfo, targetInfo, converter, converterParameter, converterCulture));

			return this;
		}

		public BindingObjectSource<TTarget> AddFuncBinding(Expression<Func<object, object>> sourceGetter, Expression<Func<TTarget, object>> targetProperty)
		{
			PropertyInfo[] targetInfo = PropertyChainFromExp.Get (targetProperty.Body);
			Bridges.Add (new FuncBindingBridge<object>(this, sourceGetter, targetInfo));

			return this;
		}


		#endregion
	}
}

