using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace QSOrmProject
{
	public abstract class PropertyChangedBase : INotifyPropertyChanged
	{
		public virtual void FirePropertyChanged ()
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke (null, null);
		}

		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged (string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				var att = this.GetType ().GetProperty (propertyName).GetCustomAttributes (typeof(PropertyChangedAlsoAttribute), true);
				handler (this, new PropertyChangedEventArgs (propertyName));
				if(att != null)
				{
					foreach(string propName in (att[0] as PropertyChangedAlsoAttribute).PropertiesNames)
						handler (this, new PropertyChangedEventArgs (propName));
				}
			}
		}

		protected bool SetField<T> (ref T field, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals (field, value))
				return false;
			field = value;
			OnPropertyChanged (propertyName);
			return true;
		}

		//No string implementation
		protected virtual void OnPropertyChanged<T> (Expression<Func<T>> selectorExpression)
		{
			if (selectorExpression == null)
				throw new ArgumentNullException ("selectorExpression");
			MemberExpression body = selectorExpression.Body as MemberExpression;
			if (body == null)
				throw new ArgumentException ("The body must be a member expression");
			OnPropertyChanged (body.Member.Name);
		}

		protected bool SetField<T> (ref T field, T value, Expression<Func<T>> selectorExpression)
		{
			if (EqualityComparer<T>.Default.Equals (field, value))
				return false;
			field = value;
			OnPropertyChanged (selectorExpression);
			return true;
		}
	}


}

