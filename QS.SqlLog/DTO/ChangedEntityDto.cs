using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;

namespace QS.SqlLog.Domain
{
	public class ChangedEntityDto : IChangedEntity {
		public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
		public EntityChangeOperation Operation { get; set; } 
		public string EntityClassName { get; set; }
		public int EntityId { get; set; }
		public string EntityTitle { get; set; }
		public ICovariantCollection<IFieldChange> Changes { get; set; } = new CovariantCollection<FieldChangeDto>();
		public IChangeSet ChangeSet { get; set; } = null;
	}
}
