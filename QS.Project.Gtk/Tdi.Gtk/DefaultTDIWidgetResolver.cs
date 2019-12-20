using System;
using Gtk;
using QS.Journal.GtkUI;
using QS.Project.Journal;
using QS.ViewModels;
using QS.Navigation.TabNavigation.TdiAdapter;
using QS.GtkUI.Navigation.TabNavigation.TdiAdapter;

namespace QS.Tdi.Gtk
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

			if(viewModel is TdiTabViewModelAdapter) {
				return new TdiTabViewModelAdapterView(this, (TdiTabViewModelAdapter)viewModel);
			}

			return null;
		}
	}
}
