using QS.DomainModel.Entity;
using QS.Project.Domain;

namespace QS.HistoryLog.Domain
{
	public class ChangeSetDto : IChangeSet {
		public string ActionName { get; set; }
		public int UserId { get; set; }
		public string UserLogin { get; set; }
		public ICovariantCollection<IChangedEntity> Entities { get; } = new CovariantCollection<ChangedEntityDto>();
		public UserBase User { get; set; } = null;
		public string UserName => User?.Name ?? UserLogin;
	}
}

