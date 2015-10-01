using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.DTO;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class HouseEntry : Entry
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private ListStore completionListStore;

		private Thread queryThread;

		public BindingControler<HouseEntry> Binding { get; private set; }

		public bool? OsmCompletion { 
			get {
				if (String.IsNullOrEmpty (osmhouse))
					return null;
				return osmhouse == House;
			}
		}

		string osmhouse;

		public string House {
			get {
				return Text;
			}
			set {
				if (Text == value)
					return;
				
				this.Text = value;
			}
		}

		OsmStreet street;

		public OsmStreet Street {
			get { 
				return street;
			}
			set {
				street = value;
				osmhouse = null;
				OnStreetSet ();
			}
		}

		public HouseEntry ()
		{
			Binding = new BindingControler<HouseEntry> (this, new Expression<Func<HouseEntry, object>>[] {
				(w => w.House),
				(w => w.OsmCompletion),
				(w => w.Text)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			this.Completion.PackStart (cell, true);
			this.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var houseNumber = (string)tree_model.GetValue (iter, 0);
			string pattern = String.Format ("{0}", Regex.Escape (Text.ToLower ()));
			houseNumber = Regex.Replace (houseNumber, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = houseNumber;
		}


		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = (string)completion.Model.GetValue (iter, 0);
			return Regex.IsMatch (val, String.Format ("{0}", Regex.Escape (key)), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			House = osmhouse = args.Model.GetValue (args.Iter, 0).ToString ();
			args.RetVal = true;
		}


		void OnStreetSet ()
		{
			if (queryThread != null && queryThread.IsAlive)
				queryThread.Abort ();
			queryThread = new Thread (fillAutocomplete);
			queryThread.IsBackground = true;
			queryThread.Start ();
		}

		private void fillAutocomplete ()
		{
			if (String.IsNullOrWhiteSpace (Street.Name)) {
				if (completionListStore != null)
					completionListStore.Clear ();
				return;
			}
				
			logger.Info ("Запрос домов на {0}...", Street.Name);
			IOsmService svc = OsmWorker.GetOsmService ();
			var houses = svc.GetHouseNumbers (Street.CityOsmId, Street.Name, Street.District);
			completionListStore = new ListStore (typeof(string));
			foreach (var h in houses) {
				completionListStore.AppendValues (h);
			}
			this.Completion.Model = completionListStore;
			logger.Debug ("Получено {0} домов...", houses.Count);
		}

		protected override void OnChanged ()
		{
			if (HasFocus && osmhouse == null)
				osmhouse = "not set";
			Binding.FireChange (w => w.House, w => w.Text, w => w.OsmCompletion);
			base.OnChanged ();
		}
	}
}

