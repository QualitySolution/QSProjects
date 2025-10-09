using System;
using System.Collections.Generic;
using NHibernate.Proxy;
using QS.DomainModel.Entity;

namespace QS.HistoryLog.Domain
{
	public class ChangedEntity : ChangedEntityBase, IChangedEntityToSave
	{
		#region Свойства

		public virtual IChangeSetToSave ChangeSet { get; set; }
		
		public virtual List<FieldChange> Changes { get; set; }
		IEnumerable<IFieldChangeToSave> IChangedEntityToSave.Changes => Changes;
		
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
