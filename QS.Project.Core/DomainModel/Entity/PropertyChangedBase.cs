using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace QS.DomainModel.Entity
{
	public abstract class PropertyChangedBase : INotifyPropertyChanged
	{
		public virtual void FirePropertyChanged ()
		{
			OnPropertyChanged(null);
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged ([CallerMemberName]string propertyName = "")
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs (propertyName));
				var propertyInfo = this.GetType().GetProperty(propertyName);
				if(propertyInfo != null) {
					var attributes = propertyInfo.GetCustomAttributes(typeof(PropertyChangedAlsoAttribute), true);
					foreach(PropertyChangedAlsoAttribute attribute in attributes)
						foreach(string propName in attribute.PropertiesNames)
							PropertyChanged(this, new PropertyChangedEventArgs(propName));
				}
			}
		}

		/// <summary>
		/// Устанавливаем поле с вызовом события обновления, при вызове внутри метода set указывать имя свойства не обязательно.
		/// </summary>
		protected bool SetField<T> (ref T field, T value, [CallerMemberName]string propertyName = "")
		{
			if (NHibernate.NHibernateUtil.IsInitialized (value)) {
				if (EqualityComparer<T>.Default.Equals (field, value))
					return false;
			}
			field = value;
			OnPropertyChanged (propertyName);
			return true;
		}

		//No string implementation
		protected void OnPropertyChanged<T> (Expression<Func<T>> selectorExpression)
		{
			OnPropertyChanged(GetPropertyName(selectorExpression));
		}

		protected bool SetField<T> (ref T field, T value, Expression<Func<T>> selectorExpression)
		{
			if (NHibernate.NHibernateUtil.IsInitialized (value)) {
				if(EqualityComparer<T>.Default.Equals (field, value))
					return false;
			}
			field = value;
			OnPropertyChanged (selectorExpression);
			return true;
		}

		protected string GetPropertyName<T>(Expression<Func<T>> selectorExpression)
		{
			if(selectorExpression == null)
				throw new ArgumentNullException("selectorExpression");
			MemberExpression body = selectorExpression.Body as MemberExpression;
			if(body == null)
				throw new ArgumentException("The body must be a member expression");
			return body.Member.Name;
		}
	}
}

