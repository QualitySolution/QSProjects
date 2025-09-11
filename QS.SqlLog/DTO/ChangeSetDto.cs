using System.Collections.Generic;

namespace QS.SqlLog.Domain
{
	public class ChangeSetDto {
		public string ActionName { get; set; }
		public int? UserId { get; set; }
		public string UserLogin { get; set; }
		public List<ChangedEntityDto> Entities { get; } = new List<ChangedEntityDto>();
	}
}

