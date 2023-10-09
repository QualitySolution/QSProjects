using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace QS.DomainModel.Entity
{
	public abstract class PropertyChangedBase : INotifyPropertyChanged
	{
		private object _lock = new object();

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

		/// <summary>
		/// Устанавливаем поле с вызовом события обновления, при вызове внутри метода set указывать имя свойства не обязательно.
		/// </summary>
		protected bool SetField<T> (ref T field, T value, [CallerMemberName]string propertyName = "")
		{
			if (NHibernate.NHibernateUtil.IsInitialized (value)) {
				if (EqualityComparer<T>.Default.Equals (field, value))
					return false;
			}

			lock(_lock) {
				field = value;
			}
			OnPropertyChanged (propertyName);
			return true;
		}

		protected bool SetField(ref int field, int value, [CallerMemberName] string propertyName = "") {
			if(NHibernate.NHibernateUtil.IsInitialized(value)) {
				if(EqualityComparer<int>.Default.Equals(field, value))
					return false;
			}

			Interlocked.Exchange(ref field, value);
			OnPropertyChanged(propertyName);
			return true;
		}

		protected bool SetField(ref float field, float value, [CallerMemberName] string propertyName = "") {
			if(NHibernate.NHibernateUtil.IsInitialized(value)) {
				if(EqualityComparer<float>.Default.Equals(field, value))
					return false;
			}

			Interlocked.Exchange(ref field, value);
			OnPropertyChanged(propertyName);
			return true;
		}

		protected bool SetField(ref double field, double value, [CallerMemberName] string propertyName = "") {
			if(NHibernate.NHibernateUtil.IsInitialized(value)) {
				if(EqualityComparer<double>.Default.Equals(field, value))
					return false;
			}

			Interlocked.Exchange(ref field, value);
			OnPropertyChanged(propertyName);
			return true;
		}

		protected bool SetField(ref long field, long value, [CallerMemberName] string propertyName = "") {
			if(NHibernate.NHibernateUtil.IsInitialized(value)) {
				if(EqualityComparer<long>.Default.Equals(field, value))
					return false;
			}

			Interlocked.Exchange(ref field, value);
			OnPropertyChanged(propertyName);
			return true;
		}

		protected bool SetField(ref object field, object value, [CallerMemberName] string propertyName = "")
		{
			if(NHibernate.NHibernateUtil.IsInitialized(value)) {
				if(EqualityComparer<object>.Default.Equals(field, value))
					return false;
			}
			Interlocked.Exchange(ref field, value);
			OnPropertyChanged(propertyName);
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
			lock(_lock) {
				field = value;
			}
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

