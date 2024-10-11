using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using QS.DomainModel.Entity;
using ReactiveUI;

namespace QS.ViewModels
{
	public abstract class ViewModelBase : ReactiveObject
	{
		#region Совместимость со старым кодом
		//Данные методы нужноы для того чтобы сразу не переделывать код.
		//Но уже по возможности начинать использовать ReactiveUI.
		//Возможно в будущим их удалим как дублирующие.
		
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
		
		protected virtual void OnPropertyChanged ([CallerMemberName]string propertyName = "")
		{
			this.RaisePropertyChanged(propertyName);
			var propertyInfos = this.GetType().GetProperties().Where(x => x.Name == propertyName);
			foreach (var propertyInfo in propertyInfos) {
				var attributes = propertyInfo.GetCustomAttributes(typeof(PropertyChangedAlsoAttribute), true);
				foreach(PropertyChangedAlsoAttribute attribute in attributes)
				foreach(string propName in attribute.PropertiesNames)
					this.RaisePropertyChanged(propName);
			}
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
		
		protected void OnPropertyChanged<T> (Expression<Func<T>> selectorExpression)
		{
			OnPropertyChanged(GetPropertyName(selectorExpression));
		}

		protected string GetPropertyName<T>(Expression<Func<T>> selectorExpression)
		{
			if(selectorExpression == null)
				throw new ArgumentNullException(nameof(selectorExpression));

			if(selectorExpression.Body is MemberExpression body)
				return body.Member.Name;

			if(selectorExpression.Body is UnaryExpression unary
			   && unary.Operand is MemberExpression unaryBody)
				return unaryBody.Member.Name;

			throw new ArgumentException("Selector must provide MemberExpression");
		}

		#endregion
	}
}
