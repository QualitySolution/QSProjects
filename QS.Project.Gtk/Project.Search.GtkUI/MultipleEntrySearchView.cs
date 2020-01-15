using System;
using System.Linq;
using Gdk;
using QS.Project.Journal.Search;

namespace QS.Project.Search.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MultipleEntrySearchView : Gtk.Bin
	{
		private readonly MultipleEntrySearchViewModel viewModel;

		public MultipleEntrySearchView(MultipleEntrySearchViewModel viewModel)
		{
			this.Build();

			this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			buttonAddAnd.Clicked += OnButtonAddAndClicked;
			buttonSearchClear.Clicked += OnButtonSearchClearClicked;

			entrySearch1.Binding.AddBinding(viewModel, vm => vm.SearchValue1, w => w.Text).InitializeFromSource();
			entrySearch2.Binding.AddBinding(viewModel, vm => vm.SearchValue2, w => w.Text).InitializeFromSource();
			entrySearch3.Binding.AddBinding(viewModel, vm => vm.SearchValue3, w => w.Text).InitializeFromSource();
			entrySearch4.Binding.AddBinding(viewModel, vm => vm.SearchValue4, w => w.Text).InitializeFromSource();

			entrySearch1.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch2.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch3.WidgetEvent += EntrySearch_WidgetEvent;
			entrySearch4.WidgetEvent += EntrySearch_WidgetEvent;

			SearchVisible(1);
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			viewModel.Clear();
		}

		private int searchEntryShown = 1;

		protected void OnButtonAddAndClicked(object sender, EventArgs e)
		{
			SearchVisible(searchEntryShown + 1);
		}

		private void SearchVisible(int count)
		{
			entrySearch1.Visible = count > 0;
			ylabelSearchAnd.Visible = entrySearch2.Visible = count > 1;
			ylabelSearchAnd2.Visible = entrySearch3.Visible = count > 2;
			ylabelSearchAnd3.Visible = entrySearch4.Visible = count > 3;
			buttonAddAnd.Sensitive = count < 4;
			searchEntryShown = count;
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
