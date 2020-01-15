using System;
using System.Linq;
using Gdk;
using QS.Project.Journal.Search;

namespace QS.Project.Search.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SingleEntrySearchView : Gtk.Bin
	{
		private SingleEntrySearchViewModel viewModel;

		public SingleEntrySearchView(SingleEntrySearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entrySearch.Binding.AddBinding(viewModel, vm => vm.SearchValue, w => w.Text).InitializeFromSource();
			entrySearch.WidgetEvent += EntrySearch_WidgetEvent;
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			viewModel.Clear();
		}

		protected void EntrySearch_WidgetEvent(object o, Gtk.WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					viewModel.ManualSearchUpdate();
				}
			}
		}
	}
}
