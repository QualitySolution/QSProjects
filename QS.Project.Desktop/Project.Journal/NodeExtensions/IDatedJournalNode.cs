using System;

namespace QS.Project.Journal.NodeExtensions {
	public interface IDatedJournalNode {
		DateTime DocumentDate { get; }
	}
}
