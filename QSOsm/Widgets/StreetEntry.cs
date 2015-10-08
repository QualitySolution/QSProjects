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
	public class StreetEntry : Entry
	{
		enum columns
		{
			Street,
			District
		}

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		private ListStore completionListStore;

		public event EventHandler StreetSelected;

		private Thread queryThread;

		public BindingControler<StreetEntry> Binding { get; private set; }

		long cityId;

		public long CityId {
			get {
				return cityId;
			}
			set {
				cityId = value;
				if (cityId != 0)
					OnCityIdSet ();
			}
		}

		string street;

		public string Street {
			get {
				return street;
			}
			set {
				street = value;
				UpdateText ();
			}
		}

		string streetDistrict;

		public string StreetDistrict {
			get {
				return streetDistrict;
			}
			set {
				streetDistrict = value;
				UpdateText ();
			}
		}

		void UpdateText ()
		{
			this.Text = String.IsNullOrWhiteSpace (StreetDistrict) ? Street : String.Format ("{0} ({1})", Street, StreetDistrict);
			OnStreetSelected ();
		}

		public StreetEntry ()
		{
			Binding = new BindingControler<StreetEntry> (this, new Expression<Func<StreetEntry, object>>[] {
				(w => w.Street), (w => w.StreetDistrict)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			this.Completion.PackStart (cell, true);
			this.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var streetName = (string)tree_model.GetValue (iter, (int)columns.Street);
			var district = (string)tree_model.GetValue (iter, (int)columns.District);
			string pattern = String.Format ("\\b{0}", Regex.Escape (Text.ToLower ()));
			streetName = Regex.Replace (streetName, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = String.IsNullOrWhiteSpace (district) ? 
				streetName : 
				String.Format ("{0} ({1})", streetName, district);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = completion.Model.GetValue (iter, (int)columns.Street).ToString ().ToLower ();
			return Regex.IsMatch (val, String.Format ("\\b{0}.*", Regex.Escape (key.ToLower ())));
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			Street = args.Model.GetValue (args.Iter, (int)columns.Street).ToString ();
			StreetDistrict = args.Model.GetValue (args.Iter, (int)columns.District).ToString ();
			args.RetVal = true;
		}

		void OnCityIdSet ()
		{
			if (queryThread != null && queryThread.IsAlive)
				queryThread.Abort ();
			queryThread = new Thread (fillAutocomplete);
			queryThread.IsBackground = true;
			queryThread.Start ();
		}

		private void fillAutocomplete ()
		{
			logger.Info ("Запрос улиц...");
			IOsmService svc = OsmWorker.GetOsmService ();
			var streets = svc.GetStreets (CityId);
			completionListStore = new ListStore (typeof(string), typeof(string));
			foreach (var s in streets) {
				completionListStore.AppendValues (
					s.Name,
					s.Districts.ToString ()
				);
			}
			this.Completion.Model = completionListStore;
			logger.Debug ("Получено {0} улиц...", streets.Count);
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Street, w => w.StreetDistrict);
			base.OnChanged ();
		}

		protected virtual void OnStreetSelected ()
		{
			if (StreetSelected != null)
				StreetSelected (null, EventArgs.Empty);
		}
	}
}

