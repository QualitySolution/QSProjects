using System;
using Gtk;
using QS.Tdi;
using QS.Project.Journal;
using QS.Views.Resolve;

namespace QS.Journal.GtkUI
{
	public static class JournalViewFactory
	{
		public static bool TryCreateView(out Widget journalView, ITdiTab tab, IGtkViewResolver viewResolver)
		{
			journalView = null;

			if(tab == null) {
				return false;
			}

			if(tab is JournalViewModelBase) {
				journalView = new JournalView((JournalViewModelBase)tab, viewResolver);
				return true;
			}
			return false;
		}
	}
}
