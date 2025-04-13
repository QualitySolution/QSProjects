using System;
using System.Collections.Generic;
namespace QS.Project.Journal
{
	public interface IJournalAction
	{
		string GetTitle(object[] selectedNodes);
		bool GetVisibility(object[] selectedNodes);
		bool GetSensitivity(object[] selectedNodes);
		Action<object[]> ExecuteAction { get; set; }
		IEnumerable<IJournalAction> ChildActions { get; }
		string HotKeys { get; }
	}
}
