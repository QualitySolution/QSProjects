using QS.DomainModel.Entity;
using QS.Project.Domain;
using System.Collections.Generic;

namespace QS.SqlLog.Domain
{
	public class ChangeSetDto : IChangeSet {
		public string ActionName { get; set; }
		public int UserId { get; set; }
		public string UserLogin { get; set; }
		public ICovariantCollection<IChangedEntity> Entities { get; } = new CovariantCollection<ChangedEntityDto>();
		public UserBase User { get; set; } = null;
		public string UserName {
			get {
				return User?.Name ?? UserLogin;
			}
		}
	}
}

