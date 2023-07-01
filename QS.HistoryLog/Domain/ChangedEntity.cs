using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using NHibernate.Proxy;
using QS.DomainModel.Entity;
using QS.Utilities.Text;

namespace QS.HistoryLog.Domain
{
	public class ChangedEntity : IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		public virtual ChangeSet ChangeSet { get; set; }
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

		public virtual IList<FieldChange> Changes { get; set; }

		public virtual string ChangeTimeText => ChangeTime.ToString("G");

		public virtual string OperationText => Operation.GetEnumTitle();

		public virtual string EntityHash => $"{EntityClassName}_{EntityId}";
		#endregion

		public ChangedEntity() { }

		public ChangedEntity(EntityChangeOperation operation, object entity, List<FieldChange> changes)
		{
			Operation = operation;
			var type = NHibernateProxyHelper.GuessClass(entity);

			EntityClassName = type.Name;
			EntityTitle = DomainHelper.GetTitle(entity);
			//Обрезаем так как в базе данных поле равно 200.
			if(EntityTitle != null && EntityTitle.Length > 200)
				EntityTitle = EntityTitle.Substring(0, 197) + "...";
			EntityId = DomainHelper.GetId(entity);
			ChangeTime = DateTime.Now;

			changes.ForEach(f => f.Entity = this);
			Changes = changes;
		}

		public virtual void AddFieldChange(FieldChange change)
		{
			change.Entity = this;
			Changes.Add(change);
		}
	}

	public enum EntityChangeOperation
	{
		[Display(Name = "Создание")]
		Create,
		[Display(Name = "Изменение")]
		Change,
		[Display(Name = "Удаление")]
		Delete
	}
}
