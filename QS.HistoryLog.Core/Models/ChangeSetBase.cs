using QS.DomainModel.Entity;

namespace QS.HistoryLog.Core.Models {
	public class ChangeSetBase : IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual string UserLogin { get; set; }

		public virtual string ActionName { get; set; }
	}
}
