using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
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
				var propertyInfos = this.GetType().GetProperties().Where(x => x.Name == propertyName);
				foreach (var propertyInfo in propertyInfos) {
					var attributes = propertyInfo.GetCustomAttributes(typeof(PropertyChangedAlsoAttribute), true);
					foreach(PropertyChangedAlsoAttribute attribute in attributes)
						foreach(string propName in attribute.PropertiesNames)
							PropertyChanged(this, new PropertyChangedEventArgs(propName));
				}
			}
		}

		protected virtual void RaisePropertyChanged(PropertyChangedEventArgs args) 
		{
			PropertyChanged?.Invoke (this, args);
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

			if(selectorExpression.Body is MemberExpression body)
				return body.Member.Name;

			if(selectorExpression.Body is UnaryExpression unary
				&& unary.Operand is MemberExpression unaryBody)
					return unaryBody.Member.Name;

			throw new ArgumentException("Selector must provide MemberExpression");
		}
	}
}

