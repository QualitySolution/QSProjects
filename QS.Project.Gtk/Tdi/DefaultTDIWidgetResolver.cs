using System;
using Gtk;
using QS.Journal.GtkUI;
using QS.Project.Journal;
using QS.ViewModels;

namespace QS.Tdi
{
	public class DefaultTDIWidgetResolver : ITDIWidgetResolver
	{
		public virtual Widget Resolve(ITdiTab tab)
		{
			if(tab is Widget) {
				return (Widget)tab;
			}

			if(tab == null) {
				return null;
			}

			if(tab is JournalViewModelBase journalTab) {
				return new JournalView(journalTab);
			}

			return null;
		}

		public virtual Widget Resolve(ViewModelBase viewModel)
		{
			if(viewModel is JournalViewModelBase journalTab) {
				return new JournalView(journalTab);
			}

			return null;
		}
	}
}
