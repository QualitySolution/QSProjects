using System;
using System.Linq;
using Gdk;
using NLog;

namespace QS.Project.Search.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SearchView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		#region Настройка
		/// <summary>
		/// Задержка в передачи запроса на поиск во view model.
		/// Измеряется в милсекундах.
		/// </summary>
		public static uint QueryDelay = 0;
		#endregion

		SearchViewModel viewModel;
		uint timerId;

		public SearchView(SearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel;
			ConfigureDlg();
		}

		void ConfigureDlg() {
			entrySearch.Changed += EntSearchText_Changed;
			entrySearch2.Changed += EntSearchText_Changed;
			entrySearch3.Changed += EntSearchText_Changed;
			entrySearch4.Changed += EntSearchText_Changed;
		}

		void EntSearchText_Changed(object sender, EventArgs e)
		{
			if(QueryDelay != 0) {
				GLib.Source.Remove(timerId);
				timerId = GLib.Timeout.Add(QueryDelay, new GLib.TimeoutHandler(RunSearch));
			} else
				RunSearch();
		}

		bool RunSearch()
		{
			var allFields = new string[] { entrySearch.Text, entrySearch2.Text, entrySearch3.Text, entrySearch4.Text };
			viewModel.SearchValues = allFields.Where(x => !String.IsNullOrEmpty(x)).ToArray();
			timerId = 0;
			return false;
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			viewModel.SearchValues = new string[0];
			entrySearch.Text = string.Empty;
			entrySearch2.Text = string.Empty;
			entrySearch3.Text = string.Empty;
			entrySearch4.Text = string.Empty;
		}

		private int searchEntryShown = 1;

		protected void OnButtonAddAndClicked(object sender, EventArgs e)
		{
			SearchVisible(searchEntryShown + 1);
		}

		private void SearchVisible(int count)
		{
			entrySearch.Visible = count > 0;
			ylabelSearchAnd.Visible = entrySearch2.Visible = count > 1;
			ylabelSearchAnd2.Visible = entrySearch3.Visible = count > 2;
			ylabelSearchAnd3.Visible = entrySearch4.Visible = count > 3;
			buttonAddAnd.Sensitive = count < 4;
			searchEntryShown = count;
		}

		protected void OnEntrySearchWidgetEvent(object o, Gtk.WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					GLib.Source.Remove(timerId);
					RunSearch();
				}
			}
		}

		protected override void OnDestroyed()
		{
			GLib.Source.Remove(timerId);
			base.OnDestroyed();
		}
	}
}