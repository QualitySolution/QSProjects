using QS.DomainModel.Entity;
using System.Collections.Generic;

namespace QS.SqlLog.Domain
{
	public class ChangeSetDto : IChangeSet {
		public string ActionName { get; set; }
		public int UserId { get; set; }
		public string UserLogin { get; set; }
		public ICovariantCollection<IChangedEntity> Entities { get; } = new CovariantCollection<ChangedEntityDto>();
	}
}

