using System;
using System.Text.RegularExpressions;
using Gtk;
using suggestionscsharp;
using System.Threading;

namespace QSFNS
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GetOrgInfo : Gtk.Bin
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private bool queryIsRunning = false;

		PartyData SelectedParty;

		public GetOrgInfo()
		{
			this.Build();

			FNSMain.SetUp();
			entryQuery.Completion = new EntryCompletion ();
			entryQuery.Completion.MinimumKeyLength = 0;
			entryQuery.Completion.MatchSelected += Completion_MatchSelected;
			entryQuery.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			entryQuery.Completion.PackStart (cell, true);
			entryQuery.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var party = (PartyData)tree_model.GetValue(iter, 0);
			string pattern = String.Format ("\\b{0}", Regex.Escape (entryQuery.Text.ToLower ()));
			var text = String.Format("{0}, {1}\n{2}", party.inn, party.name.full_with_opf, party.address.value);
			var displaytext = Regex.Replace (text, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			displaytext = displaytext.Replace("&", "&amp;");
			(cell as CellRendererText).Markup = displaytext;
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
			//var val = completion.Model.GetValue (iter, (int)columns.City).ToString ().ToLower ();
			//return Regex.IsMatch (val, String.Format ("\\b{0}.*", Regex.Escape (this.Text.ToLower ())));
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			SelectedParty = (PartyData)args.Model.GetValue(args.Iter, 0);
			entryQuery.Text = String.Format("{0}, {1}", SelectedParty.inn, SelectedParty.name);
			args.RetVal = true;
		}
	
		private void fillAutocomplete ()
		{
			logger.Info ("Запрос контрагента по [{0}]...", entryQuery.Text);
			var response = FNSMain.Api.QueryParty(entryQuery.Text);

			var completionListStore = new ListStore (typeof(PartyData));

			foreach (var sugg in response.suggestions) {
				completionListStore.AppendValues (
					sugg.data
				);
			}
			logger.Debug ("Получено {0} подсказок...", response.suggestions.Count);
			Application.Invoke(delegate {
				entryQuery.Completion.Model = completionListStore;
				queryIsRunning = false;
				if (this.HasFocus)
					entryQuery.Completion.Complete ();
			});
		}

		protected void OnEntryQueryTextInserted(object o, TextInsertedArgs args)
		{
			if (entryQuery.HasFocus && !queryIsRunning) {
				Thread queryThread = new Thread (fillAutocomplete);
				queryThread.IsBackground = true;
				queryIsRunning = true;
				queryThread.Start ();
			}
		}
	}
}

