using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Gamma.Utilities;
using NHibernate.Event;
using QS.DomainModel.Entity;
using QS.Project.DB;

namespace QS.HistoryLog.Domain
{
	public class FieldChange : IDomainObject
	{
		#region Конфигурация
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public virtual IDiffFormatter DiffFormatter { get; set; }

		#endregion

		#region Свойства

		public virtual int Id { get; set; }

		public virtual ChangedEntity Entity { get; set; }

		public virtual string Path { get; set; }
		public virtual FieldChangeType Type { get; set; }
		public virtual string OldValue { get; set; }
		public virtual string NewValue { get; set; }
		public virtual int? OldId { get; set; }
		public virtual int? NewId { get; set; }

		#endregion

		#region Расчетные

		string oldFormatedDiffText;
		public virtual string OldFormatedDiffText {
			get {
				if(!isDiffMade)
					MakeDiff();
				return oldFormatedDiffText;
			}
			protected set {
				oldFormatedDiffText = value;
			}
		}

		string newFormatedDiffText;
		public virtual string NewFormatedDiffText {
			get {
				if(!isDiffMade)
					MakeDiff();
				return newFormatedDiffText;
			}
			protected set {
				newFormatedDiffText = value;
			}
		}

		public virtual string OldValueText => ValueDisplay(OldValue);
		public virtual string NewValueText => ValueDisplay(NewValue);


		public virtual string FieldTitle {
			get { return HistoryMain.ResolveFieldTilte(Entity.EntityClassName, Path); }
		}

		public virtual string TypeText {
			get {
				return Type.GetEnumTitle();
			}
		}
		#endregion

		public FieldChange()
		{
		}

		#region Внутренние методы

		private bool isDiffMade = false;
		private void MakeDiff()
		{
			if(DiffFormatter == null)
				return;

			DiffFormatter.SideBySideDiff(OldValueText, NewValueText, out oldFormatedDiffText, out newFormatedDiffText);
			isDiffMade = true;
		}

		private void UpdateType()
		{
			if(OldId.HasValue || NewId.HasValue) {
				if(OldId.HasValue && NewId.HasValue)
					Type = FieldChangeType.Changed;
				else if(OldId.HasValue)
					Type = FieldChangeType.Removed;
				else if(NewId.HasValue)
					Type = FieldChangeType.Added;
				else
					Type = FieldChangeType.Unchanged;
			} else {
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

		#region Статические методы

		public static FieldChange CheckChange(int i, PostUpdateEvent ue)
		{
			return CreateChange(ue.State[i], ue.OldState[i], ue.Persister, i);
		}

		public static FieldChange CheckChange(int i, PostInsertEvent ie)
		{
			return CreateChange(ie.State[i], null, ie.Persister, i);
		}

		private static FieldChange CreateChange(object valueNew, object valueOld, NHibernate.Persister.Entity.IEntityPersister persister, int i)
		{
			if(valueOld == null && valueNew == null)
				return null;

			NHibernate.Type.IType propType = persister.PropertyTypes[i];
			string propName = persister.PropertyNames[i];

			var propInfo = persister.MappedClass.GetProperty(propName);
			if(propInfo.GetCustomAttributes(typeof(IgnoreHistoryTraceAttribute), true).Length > 0)
				return null;

			FieldChange change = null;

			#region Обработка в зависимости от типа данных

			if(propType is NHibernate.Type.StringType && !StringCompare(ref change, (string)valueOld, (string)valueNew))
				return null;

			var link = propType as NHibernate.Type.ManyToOneType;
			if(link != null) {
				if(!EntityCompare(ref change, valueOld, valueNew))
					return null;
			}

			if((propType is NHibernate.Type.DateTimeType || propType is NHibernate.Type.TimestampType) && !DateTimeCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.DecimalType && !DecimalCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.BooleanType && !BooleanCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.Int16Type && !IntCompare<Int16>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.Int32Type && !IntCompare<Int32>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.Int64Type && !IntCompare<Int64>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.UInt16Type && !IntCompare<UInt16>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.UInt32Type && !IntCompare<UInt32>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.UInt64Type && !IntCompare<UInt64>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is NHibernate.Type.EnumStringType && !EnumCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			#endregion

			if(change != null) {
				change.Path = propName;
				change.UpdateType();
				return change;
			}

			logger.Warn("Трекер не умеет сравнивать изменения в полях типа {0}. Поле {1} пропущено.", propType, propName);
			return null;
		}

		#endregion

		#region Методы сравнения для разных типов

		static bool StringCompare(ref FieldChange change, string valueOld, string valueNew)
		{
			if(String.IsNullOrWhiteSpace(valueNew) && String.IsNullOrWhiteSpace(valueOld))
				return false;

			if(String.Equals(valueOld, valueNew))
				return false;

			change = new FieldChange();
			change.OldValue = valueOld;
			change.NewValue = valueNew;
			return true;
		}

		static bool EntityCompare(ref FieldChange change, object valueOld, object valueNew)
		{
			if(DomainHelper.EqualDomainObjects(valueOld, valueNew))
				return false;

			change = new FieldChange();
			if(valueOld != null) {
				change.OldValue = HistoryMain.GetObjectTilte(valueOld);
				change.OldId = DomainHelper.GetId(valueOld);
			}
			if(valueNew != null) {
				change.NewValue = HistoryMain.GetObjectTilte(valueNew);
				change.NewId = DomainHelper.GetId(valueNew);
			}
			return true;
		}

		static bool DateTimeCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var dateOld = valueOld as DateTime?;
			var dateNew = valueNew as DateTime?;

			if(dateOld != null && dateNew != null && DateTime.Equals(dateOld.Value, dateNew.Value))
				return false;

			var dateOnly = info.GetCustomAttributes(typeof(HistoryDateOnlyAttribute), true).Length > 0;

			change = new FieldChange();
			if(dateOld != null)
				change.OldValue = dateOnly ? dateOld.Value.ToShortDateString() : dateOld.Value.ToString();
			if(dateNew != null)
				change.NewValue = dateOnly ? dateNew.Value.ToShortDateString() : dateNew.Value.ToString();
			return true;
		}

		static bool DecimalCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var numberOld = valueOld as Decimal?;
			var numberNew = valueNew as Decimal?;

			if(numberOld != null && numberNew != null && Decimal.Equals(numberOld.Value, numberNew.Value))
				return false;

			change = new FieldChange();
			if(numberOld != null)
				change.OldValue = numberOld.Value.ToString("G20");
			if(numberNew != null)
				change.NewValue = numberNew.Value.ToString("G20");
			return true;
		}

		static bool IntCompare<TNumber>(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) where TNumber : struct
		{
			var numberOld = valueOld as TNumber?;
			var numberNew = valueNew as TNumber?;

			if(numberOld != null && numberNew != null && Equals(numberOld.Value, numberNew.Value))
				return false;

			change = new FieldChange();
			if(numberOld != null)
				change.OldValue = String.Format("{0:D}", numberOld);
			if(numberNew != null)
				change.NewValue = String.Format("{0:D}", numberNew);
			return true;
		}

		static bool BooleanCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			var boolOld = valueOld as bool?;
			var boolNew = valueNew as bool?;

			if(boolOld != null && boolNew != null && bool.Equals(boolOld.Value, boolNew.Value))
				return false;

			change = new FieldChange();
			if(boolOld != null)
				change.OldValue = boolOld.Value.ToString();
			if(boolNew != null)
				change.NewValue = boolNew.Value.ToString();
			return true;
		}

		static bool EnumCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew)
		{
			if(valueOld != null && valueNew != null && Enum.Equals(valueOld, valueNew))
				return false;

			change = new FieldChange();
			change.OldValue = valueOld?.ToString();
			change.NewValue = valueNew?.ToString();
			return true;
		}

		#endregion

		#region Методы отображения разных типов

		string ValueDisplay(string value)
		{
			var claz = OrmConfig.FindMappingByShortClassName(Entity.EntityClassName);
			var property = GetPropertyOrNull(claz, Path);
			if(property != null) {
				if(property.Type is NHibernate.Type.BooleanType)
					return BooleanDisplay(value);
				if(property.Type is NHibernate.Type.EnumStringType)
					return EnumDisplay(value, property);
			}
			return value;
		}

		static string EnumDisplay(string value, NHibernate.Mapping.Property property)
		{
			if(String.IsNullOrWhiteSpace(value))
				return null;

			var enumType = property.Type.ReturnedClass;
			var enumValues = enumType.GetFields();

			return enumValues.FirstOrDefault(f => f.Name == value)?.GetEnumTitle();
		}

		static string BooleanDisplay(string value)
		{
			if(value == "True")
				return "Да";
			else if(value == "False")
				return "Нет";
			else
				return null;
		}

		NHibernate.Mapping.Property GetPropertyOrNull(NHibernate.Mapping.PersistentClass classMapping, string propertyName)
		{
			try {
				return classMapping?.GetProperty(propertyName);
			} catch(NHibernate.MappingException) {
				return null;
			}
		}

		#endregion
	}

	public enum FieldChangeType
	{
		[Display(Name = "Добавлено")]
		Added,
		[Display(Name = "Изменено")]
		Changed,
		[Display(Name = "Очищено")]
		Removed,
		[Display(Name = "Без изменений")]
		Unchanged
	}

	public class FieldChangeTypeStringType : NHibernate.Type.EnumStringType
	{
		public FieldChangeTypeStringType() : base(typeof(FieldChangeType))
		{
		}
	}
}