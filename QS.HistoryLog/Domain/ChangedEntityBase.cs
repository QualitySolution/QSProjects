using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using NHibernate.Type;
using QS.DomainModel.Entity;
using QS.Utilities.Text;

namespace QS.HistoryLog.Domain
{
	public abstract class ChangedEntityBase : IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual DateTime ChangeTime { get; set; }
		public virtual EntityChangeOperation Operation { get; set; }

		public virtual string EntityClassName { get; set; }
		public virtual int EntityId { get; set; }
		public virtual string EntityTitle { get; set; }

		public virtual string ObjectTitle {
			get {
				var clazz = HistoryMain.FineEntityClass(EntityClassName);
				if(clazz == null)
					return EntityClassName;
				return DomainHelper.GetSubjectNames(clazz)?.Nominative.StringToTitleCase() ?? EntityClassName;
			}
		}

		public virtual string ChangeTimeText => ChangeTime.ToString("G");

		public virtual string OperationText => Operation.GetEnumTitle();
		
		public virtual string EntityHash => $"{EntityClassName}_{EntityId}";

		#endregion
	}

	public enum EntityChangeOperation {
		[Display(Name = "Создание")]
		Create,
		[Display(Name = "Изменение")]
		Change,
		[Display(Name = "Удаление")]
		Delete
	}
}
