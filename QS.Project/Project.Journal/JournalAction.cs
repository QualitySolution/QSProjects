using System;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public class JournalAction : IJournalAction
	{
		public JournalAction(string title, Func<object[], bool> sensitiveFunc, Func<object[], bool> visibleFunc, Action<object[]> executeAction)
		{
			ChildActionsList = new List<IJournalAction>();
			Title = title;
			this.sensitiveFunc = sensitiveFunc;
			this.visibleFunc = visibleFunc;
			ExecuteAction = executeAction;
		}

		public string Title { get; }
		private readonly Func<object[], bool> sensitiveFunc;
		private readonly Func<object[], bool> visibleFunc;
		public IEnumerable<IJournalAction> ChildActions => ChildActionsList;
		public List<IJournalAction> ChildActionsList { get; set; }

		public Action<object[]> ExecuteAction { get; }

		public bool GetSensitivity(object[] selectedNodes)
		{
			return sensitiveFunc.Invoke(selectedNodes);
		}

		public bool GetVisibility(object[] selectedNodes)
		{
			return visibleFunc.Invoke(selectedNodes);
		}
	}
}
