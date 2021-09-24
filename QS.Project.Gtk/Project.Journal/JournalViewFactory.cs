using System;
using Gtk;
using QS.Project.Journal.Views;
using QS.Tdi;

namespace QS.Project.Journal
{
	public static class JournalViewFactory
	{
		public static bool TryCreateView(out Widget journalView, ITdiTab tab)
		{
			journalView = null;

			if(tab == null) {
				return false;
			}

			if(tab is JournalViewModelBase) {
				journalView = new JournalView((JournalViewModelBase)tab);
				return true;
			}
			return false;
		}
	}
}
