using System;
using System.Collections.Generic;
namespace QS.Project.Journal
{
	public interface IJournalAction
	{
		string Title { get; }
		bool GetVisibility(object[] selectedNodes);
		bool GetSensitivity(object[] selectedNodes);
		Action<object[]> ExecuteAction { get; }
		IEnumerable<IJournalAction> ChildActions { get; }
	}
}
