using System;
using System.Linq;

namespace QS.Project.Journal
{
	public class JournalSelectedEventArgs : EventArgs
	{
		public object[] SelectedObjects { get; }

		public JournalSelectedEventArgs(object[] selectedobjects)
		{
			SelectedObjects = selectedobjects;
		}

		public T[] GetSelectedObjects<T>() => SelectedObjects.OfType<T>().ToArray();
	}
}
