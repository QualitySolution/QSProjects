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
			entSearchText.Changed += EntSearchText_Changed;
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
							viewModel.SearchString = entSearchText.Text;
						});
					} catch(Exception ex) {
						logger.Error(ex, $"Ошибка во время ввода строки поиска");
					} finally {
						searchInProgress = false;
					}
				});
			}
		}

		protected void OnBtnClearClicked(object sender, EventArgs e)
		{
			viewModel.SearchString = string.Empty; 
			entSearchText.Text = string.Empty;
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