using System;
namespace QS.Project.Journal
{
	public class JournalSelectedEventArgs : EventArgs
	{
		public object[] SelectedObjects { get; }

		public JournalSelectedEventArgs(object[] selectedobjects)
		{
			SelectedObjects = selectedobjects;
		}
	}
}
