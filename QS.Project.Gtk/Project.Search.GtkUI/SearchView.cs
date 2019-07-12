using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gdk;
using Gtk;
using NLog;

namespace QS.Project.Search.GtkUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SearchView : Gtk.Bin
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		SearchViewModel viewModel;
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

		DateTime lastChangedTime = DateTime.Now;
		bool searchInProgress = false;
		private CancellationTokenSource cts = new CancellationTokenSource();

		void EntSearchText_Changed(object sender, EventArgs e)
		{
			lastChangedTime = DateTime.Now;

			if(!searchInProgress) {
				Task.Run(() => {
					searchInProgress = true;
					try {
						while((DateTime.Now - lastChangedTime).TotalMilliseconds < 1500) {
							if(cts.IsCancellationRequested) {
								return;
							}
						}
						Application.Invoke((s, arg) => {
							viewModel.SearchValues = new string[] { entrySearch.Text, entrySearch2.Text, entrySearch3.Text, entrySearch4.Text };
						});
					} catch(Exception ex) {
						logger.Error(ex, $"Ошибка во время ввода строки поиска");
					} finally {
						searchInProgress = false;
					}
				});
			}
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

		protected void OnEntSearchTextWidgetEvent(object o, Gtk.WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					viewModel.Update();
				}
			}
		}

		public override void Destroy()
		{
			cts.Cancel();
			base.Destroy();
		}
	}
}