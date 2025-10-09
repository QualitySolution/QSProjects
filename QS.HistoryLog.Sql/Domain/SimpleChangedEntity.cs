using System;
using System.Collections.Generic;

namespace QS.HistoryLog.Domain
{
	public class SimpleChangedEntity : IChangedEntityToSave {
		public DateTime ChangeTime { get; set; }
		public EntityChangeOperation Operation { get; set; } 
		public string EntityClassName { get; set; }
		public int EntityId { get; set; }
		public string EntityTitle { get; set; }
		public List<SimpleFieldChange> Changes { get; set; } = new List<SimpleFieldChange>();
		IEnumerable<IFieldChangeToSave> IChangedEntityToSave.Changes => Changes;
	}
}
