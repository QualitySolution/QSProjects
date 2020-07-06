using System;
using System.Linq;
using Gdk;
using NLog;

namespace QS.Project.Search.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OneEntrySearchView : Gtk.Bin
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

		public OneEntrySearchView(SearchViewModel viewModel)
		{
			this.Build();
			this.viewModel = viewModel;
			entrySearch.Changed += EntSearchText_Changed;
		}

		public OneEntrySearchView() : base()
		{
		}

		public OneEntrySearchView(IntPtr raw) : base(raw)
		{
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
			var allFields = entrySearch.Text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
			viewModel.SearchValues = allFields;
			timerId = 0;
			return false;
		}

		protected void OnButtonSearchClearClicked(object sender, EventArgs e)
		{
			viewModel.SearchValues = new string[0];
			entrySearch.Text = string.Empty;
		}

		public override void Destroy()
		{
			GLib.Source.Remove(timerId);
			base.Destroy();
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
	}
}