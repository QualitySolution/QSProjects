using System;
using System.Collections.Generic;

namespace QS.Project.Journal
{
	public class JournalSelectedNodesEventArgs : EventArgs
	{
		public IList<JournalEntityNodeBase> SelectedNodes { get; }

		public JournalSelectedNodesEventArgs(IList<JournalEntityNodeBase> selectedNodes)
		{
			SelectedNodes = selectedNodes;
		}
	}
}
