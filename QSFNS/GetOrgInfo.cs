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
		bool noQuery = false;

		PartyData selectedParty;

		public PartyData SelectedParty
		{
			get
			{
				return selectedParty;
			}
			set
			{
				selectedParty = value;
				if(selectedParty != null)
				{
					noQuery = true;
					entryQuery.Text = String.Format("{0}, {1}", selectedParty.inn, selectedParty.name.short_with_opf);
					noQuery = false;
				}
				buttonFillFields.Sensitive = selectedParty != null;
			}
		}

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
			var text = String.Format("{0}, {1}\n{2}", party.inn, party.name.short_with_opf, party.address.value);
			var displaytext = Regex.Replace (text, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			displaytext = displaytext.Replace("&", "&amp;");
			(cell as CellRendererText).Markup = displaytext;
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			return true;
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			SelectedParty = (PartyData)args.Model.GetValue(args.Iter, 0);
			args.RetVal = true;
		}
	
		private void fillAutocomplete ()
		{
			var response = FNSMain.CachedQueryParty(entryQuery.Text);

			var completionListStore = new ListStore (typeof(PartyData));

			foreach (var sugg in response.suggestions) {
				completionListStore.AppendValues (
					sugg.data
				);
			}

			Application.Invoke(delegate {
				entryQuery.Completion.Model = completionListStore;
				queryIsRunning = false;
				if (this.HasFocus)
					entryQuery.Completion.Complete ();
			});
		}

		void RunQuery()
		{
			if (entryQuery.HasFocus && !queryIsRunning && !noQuery) {
				SelectedParty = null;

				Thread queryThread = new Thread (fillAutocomplete);
				queryThread.IsBackground = true;
				queryIsRunning = true;
				queryThread.Start ();
			}
		}

		protected void OnEntryQueryTextInserted(object o, TextInsertedArgs args)
		{
			RunQuery();
		}

		protected void OnEntryQueryTextDeleted(object o, TextDeletedArgs args)
		{
			RunQuery();
		}
	}
}

