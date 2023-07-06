using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using FluentNHibernate.Utils;
using NHibernate.Event;
using NHibernate.Type;
using NHibernate.Util;
using QS.DomainModel.Entity;
using QS.DomainModel.Tracking;
using QS.Project.DB;

namespace QS.HistoryLog.Domain
{
	public class FieldChange : FieldChangeBase
	{
		#region Конфигурация

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#endregion

		#region Свойства

		public virtual ChangedEntity Entity { get; set; }

		#endregion

		#region Расчетные

		public virtual string OldValueText => ValueDisplay(OldValue);
		public virtual string NewValueText => ValueDisplay(NewValue);

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

		public virtual string FieldTitle {
			get { return HistoryMain.ResolveFieldTitle(Entity.EntityClassName, Path); }
		}

		#endregion

		public FieldChange()
		{
		}

		#region Внутренние методы

		private bool isDiffMade = false;
		private void MakeDiff() {
			if(DiffFormatter == null)
				return;

			DiffFormatter.SideBySideDiff(OldValueText, NewValueText, out oldFormatedDiffText, out newFormatedDiffText);
			isDiffMade = true;
		}

		#endregion

		#region Методы сравнения для разных типов

		private static bool StringCompare(ref FieldChange change, string valueOld, string valueNew) {
			if(String.IsNullOrWhiteSpace(valueNew) && String.IsNullOrWhiteSpace(valueOld))
				return false;

			if(String.Equals(valueOld, valueNew))
				return false;

			change = new FieldChange();
			change.OldValue = valueOld;
			change.NewValue = valueNew;
			return true;
		}

		private static bool EntityCompare(ref FieldChange change, object valueOld, object valueNew) {
			if(DomainHelper.EqualDomainObjects(valueOld, valueNew))
				return false;

			change = new FieldChange();
			if(valueOld != null) {
				change.OldValue = GetObjectTitle(valueOld);
				change.OldId = DomainHelper.GetId(valueOld);
			}
			if(valueNew != null) {
				change.NewValue = GetObjectTitle(valueNew);
				change.NewId = DomainHelper.GetId(valueNew);
			}
			return true;
		}

		private static bool DateTimeCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) {
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

		private static bool DecimalCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) {
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

		private static bool IntCompare<TNumber>(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) where TNumber : struct {
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

		private static bool BooleanCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) {
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

		protected static bool EnumCompare(ref FieldChange change, PropertyInfo info, object valueOld, object valueNew) {
			if(valueOld != null && valueNew != null && Enum.Equals(valueOld, valueNew))
				return false;

			change = new FieldChange();
			change.OldValue = valueOld?.ToString();
			change.NewValue = valueNew?.ToString();
			return true;
		}

		#endregion

		#region Статические методы

		public static FieldChange CheckChange(IUnitOfWorkTracked uow, int i, PostUpdateEvent ue) {
			return CreateChange(uow, ue.State[i], ue.OldState[i], ue.Persister, i);
		}

		public static FieldChange CheckChange(IUnitOfWorkTracked uow, int i, PostInsertEvent ie) {
			return CreateChange(uow, ie.State[i], null, ie.Persister, i);
		}

		private static FieldChange CreateChange(IUnitOfWorkTracked uow, object valueNew, object valueOld, NHibernate.Persister.Entity.IEntityPersister persister, int i) {
			if(valueOld == null && valueNew == null)
				return null;

			IType propType = persister.PropertyTypes[i];
			string propName = persister.PropertyNames[i];

			var propInfo = persister.MappedClass.GetProperty(propName);
			if(propInfo.GetCustomAttributes(typeof(IgnoreHistoryTraceAttribute), true).Length > 0)
				return null;

			var historyIdentifierAttributeInfo = propInfo.GetCustomAttribute<HistoryIdentifierAttribute>();

			FieldChange change = null;

			if(historyIdentifierAttributeInfo != null) {
				return RefIdCompare(uow, ref valueNew, ref valueOld, propName, historyIdentifierAttributeInfo, ref change);
			}

			#region Обработка в зависимости от типа данных

			if(propType is StringType && !StringCompare(ref change, (string)valueOld, (string)valueNew))
				return null;

			var link = propType as ManyToOneType;
			if(link != null) {
				if(!EntityCompare(ref change, valueOld, valueNew))
					return null;
			}

			if((propType is DateTimeType || propType is TimestampType) && !DateTimeCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is DecimalType && !DecimalCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is BooleanType && !BooleanCompare(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is Int16Type && !IntCompare<Int16>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is Int32Type && !IntCompare<Int32>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is Int64Type && !IntCompare<Int64>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is UInt16Type && !IntCompare<UInt16>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is UInt32Type && !IntCompare<UInt32>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is UInt64Type && !IntCompare<UInt64>(ref change, propInfo, valueOld, valueNew))
				return null;

			if(propType is EnumStringType && !EnumCompare(ref change, propInfo, valueOld, valueNew))
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

		private static FieldChange RefIdCompare(
			IUnitOfWorkTracked uow,
			ref object valueNew,
			ref object valueOld,
			string propName,
			HistoryIdentifierAttribute historyIdentifierAttributeInfo,
			ref FieldChange change)
		{
			if(valueOld == valueNew) {
				return null;
			}

			if(valueOld != null) {
				IDomainObject oldEntity = (IDomainObject)uow.Session.Get(historyIdentifierAttributeInfo.TargetType, valueOld);
				valueOld = $"[{valueOld}][\"{oldEntity.GetTitle()}\"]";
			}

			if(valueNew != null) {
				IDomainObject newEntity = (IDomainObject)uow.Session.Get(historyIdentifierAttributeInfo.TargetType, valueNew);
				valueNew = $"[{valueNew}][\"{newEntity.GetTitle()}\"]";
			}

			if(!StringCompare(ref change, (string)valueOld, (string)valueNew)) {
				return null;
			}
			else {
				change.Path = propName;
				change.UpdateType();
				return change;
			}
		}

		#endregion

		#region Методы отображения разных типов

		protected string ValueDisplay(string value) {
			var claz = OrmConfig.FindMappingByShortClassName(Entity.EntityClassName);
			var property = GetPropertyOrNull(claz, Path);
			if(property != null) {
				if(property.Type is BooleanType)
					return BooleanDisplay(value);
				if(property.Type is EnumStringType)
					return EnumDisplay(value, property);
			}
			return value;
		}

		#endregion
	}
}
