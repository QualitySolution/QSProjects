using System;
using System.Collections.Generic;
namespace QS.Project.Journal
{
	public interface IJournalAction
	{
		string Title { get; }
		bool GetVisibility(IList<object> selectedNodes);
		bool GetSensitivity(IList<object> selectedNodes);
		Action<IList<object>> ExecuteAction { get; }
		IEnumerable<IJournalAction> ChildActions { get; }
		string HotKeys { get; }
	}
}
