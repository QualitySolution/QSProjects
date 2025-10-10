using System.Collections.Generic;

namespace QS.HistoryLog.Domain {
	public interface IChangeSetToSave {
		string ActionName { get; }
		int UserId { get; }
		string UserLogin { get; }
		IEnumerable<IChangedEntityToSave> Entities { get; } 
	}
}
