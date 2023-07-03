using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using NHibernate.Proxy;
using QS.DomainModel.Entity;
using QS.Utilities.Text;

namespace QS.HistoryLog.Domain
{
	public class ChangedEntity : ChangedEntityBase
	{
		#region Свойства

		public virtual ChangeSet ChangeSet { get; set; }
		
		public virtual IList<FieldChange> Changes { get; set; }
		
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
}
