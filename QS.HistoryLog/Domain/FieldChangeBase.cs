using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Mapping;
using NHibernate.Type;
using QS.DomainModel.Entity;

namespace QS.HistoryLog.Domain
{
	public abstract class FieldChangeBase : IDomainObject
	{
		#region Конфигурация

		public virtual IDiffFormatter DiffFormatter { get; set; }

		#endregion

		#region Свойства

		public virtual int Id { get; set; }

		public virtual string Path { get; set; }
		public virtual FieldChangeType Type { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }
		public virtual int? OldId { get; set; }
		public virtual int? NewId { get; set; }

		#endregion

		#region Расчетные

		public virtual string TypeText {
			get {
				return Type.GetEnumTitle();
			}
		}

		#endregion

		#region Внутренние методы

		protected static string GetObjectTitle(object value) => HistoryMain.GetObjectTitle(value);

		protected void UpdateType() {
			if(OldId.HasValue || NewId.HasValue) {
				if(OldId.HasValue && NewId.HasValue)
					Type = FieldChangeType.Changed;
				else if(OldId.HasValue)
					Type = FieldChangeType.Removed;
				else if(NewId.HasValue)
					Type = FieldChangeType.Added;
				else
					Type = FieldChangeType.Unchanged;
			}
			else {
				if(!String.IsNullOrWhiteSpace(OldValue) && !String.IsNullOrWhiteSpace(NewValue))
					Type = FieldChangeType.Changed;
				else if(String.IsNullOrWhiteSpace(OldValue))
					Type = FieldChangeType.Added;
				else if(String.IsNullOrWhiteSpace(NewValue))
					Type = FieldChangeType.Removed;
				else
					Type = FieldChangeType.Unchanged;
			}
		}

		#endregion

		#region Методы отображения разных типов

		protected string EnumDisplay(string value, Property property) {
			if(String.IsNullOrWhiteSpace(value))
				return null;

			var enumType = property.Type.ReturnedClass;
			var enumValues = enumType.GetFields();

			return enumValues.FirstOrDefault(f => f.Name == value)?.GetEnumTitle();
		}

		protected static string BooleanDisplay(string value) {
			if(value == "True")
				return "Да";
			else if(value == "False")
				return "Нет";
			else
				return null;
		}

		protected Property GetPropertyOrNull(PersistentClass classMapping, string propertyName) {
			try {
				return classMapping?.GetProperty(propertyName);
			}
			catch(NHibernate.MappingException) {
				return null;
			}
		}

		#endregion
	}

	public enum FieldChangeType {
		[Display(Name = "Добавлено")]
		Added,
		[Display(Name = "Изменено")]
		Changed,
		[Display(Name = "Очищено")]
		Removed,
		[Display(Name = "Без изменений")]
		Unchanged
	}
}
