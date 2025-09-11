using System;
using System.Collections.Generic;

namespace QS.SqlLog.Domain
{
	public class ChangedEntityDto {
		public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
		public string Operation { get; set; } 
		public string EntityClassName { get; set; }
		public int EntityId { get; set; }
		public string EntityTitle { get; set; }
		public List<FieldChangeDto> Changes { get; } = new List<FieldChangeDto>();
	}
}
