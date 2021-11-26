using System;
using System.Collections.Generic;
using System.Linq;

namespace QS.Project.Journal
{
	public class JournalSelectedEventArgs : EventArgs
	{
		public IList<object> SelectedObjects { get; }

		public JournalSelectedEventArgs(IList<object> selectedobjects)
		{
			SelectedObjects = selectedobjects;
		}

		public T[] GetSelectedObjects<T>() => SelectedObjects.OfType<T>().ToArray();
	}
}
