using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace QS.ViewModels.Control
{
	public class PropertyBinder<TBindedEntity, TProperty> : IPropertyBinder<TProperty>
		where TBindedEntity : class, INotifyPropertyChanged
	{
		TBindedEntity bindedEntity;
		readonly PropertyInfo propertyInfo;

		public PropertyBinder(TBindedEntity bindedEntity, Expression<Func<TBindedEntity, TProperty>> bindedProperty)
		{
			this.bindedEntity = bindedEntity ?? throw new ArgumentNullException(nameof(bindedEntity));
			propertyInfo = Gamma.Utilities.PropertyUtil.GetPropertyInfo(bindedProperty);
			bindedEntity.PropertyChanged += BindedEntity_PropertyChanged;
		}

		public TProperty PropertyValue {
			get => (TProperty)propertyInfo.GetValue(bindedEntity);
			set { if(!object.ReferenceEquals(PropertyValue, value) )
				propertyInfo.SetValue(bindedEntity, value);
			}
		}

		public event EventHandler Changed;

		void BindedEntity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == propertyInfo.Name)
				Changed?.Invoke(this, EventArgs.Empty);
		}

		public void Dispose() {
			if(bindedEntity is null) {
				return;
			}
			bindedEntity.PropertyChanged -= BindedEntity_PropertyChanged;
			bindedEntity = null;
		}
	}
}
