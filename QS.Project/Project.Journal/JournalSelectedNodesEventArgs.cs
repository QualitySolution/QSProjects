using System;
namespace QS.Project.Journal
{
	public class JournalSelectedNodesEventArgs : EventArgs
	{
		public JournalEntityNodeBase[] SelectedNodes { get; }

		public JournalSelectedNodesEventArgs(JournalEntityNodeBase[] selectedNodes)
		{
			SelectedNodes = selectedNodes;
		}
	}
}
