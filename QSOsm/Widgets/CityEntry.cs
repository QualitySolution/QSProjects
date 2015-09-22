using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class CityEntry : Entry
	{
		const int CityColumn = 1;
		const int DistrictColumn = 2;
		const int OsmIdColumn = 3;

		public event EventHandler CitySelected;

		private ListStore completionListStore;

		private bool queryIsRunning = false;

		public BindingControler<CityEntry> Binding { get; private set; }

		long osmId;

		public long OsmId { get { return osmId; } }

		string city;

		public string City {
			get {
				return city;
			}
			set {
				city = value;
				UpdateText ();
			}
		}

		string cityDistrict;

		public string CityDistrict {
			get {
				return cityDistrict;
			}
			set {
				cityDistrict = value;
				UpdateText ();
			}
		}

		void UpdateText ()
		{
			this.Text = String.IsNullOrWhiteSpace (CityDistrict) ? City : String.Format ("{0} ({1})", City, CityDistrict);
			if (osmId == default(long) && City != default(string) && CityDistrict != default(string)) {
				IOsmService svc = OsmWorker.GetOsmService ();
				osmId = svc.GetCityId (City, CityDistrict);
				OnCitySelected ();
			}
		}

		public CityEntry ()
		{
			Binding = new BindingControler<CityEntry> (this, new Expression<Func<CityEntry, object>>[] {
				(w => w.City), (w => w.CityDistrict)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.TextColumn = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.TextInserted += CityEntryTextInserted;
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			City = args.Model.GetValue (args.Iter, CityColumn).ToString ();
			CityDistrict = args.Model.GetValue (args.Iter, DistrictColumn).ToString ();
			osmId = (long)args.Model.GetValue (args.Iter, OsmIdColumn);
			OnCitySelected ();
		}

		protected virtual void OnCitySelected ()
		{
			if (CitySelected != null)
				CitySelected (null, EventArgs.Empty);
		}

		void CityEntryTextInserted (object o, TextInsertedArgs args)
		{
			if (completionListStore == null && !queryIsRunning) {
				Thread queryThread = new Thread (fillAutocomplete);
				queryThread.IsBackground = true;
				queryIsRunning = true;
				queryThread.Start ();
			}
		}

		private void fillAutocomplete ()
		{
			IOsmService svc = OsmWorker.GetOsmService ();
			var cities = svc.GetCities ();
			completionListStore = new ListStore (typeof(string), typeof(string), typeof(string), typeof(long));
			foreach (var c in cities) {
				completionListStore.AppendValues (
					String.IsNullOrWhiteSpace (c.SuburbDistrict) ? c.Name : String.Format ("{0} ({1})", c.Name, c.SuburbDistrict),
					c.Name,
					c.SuburbDistrict,
					c.OsmId
				);
			}
			this.Completion.Model = completionListStore;
			queryIsRunning = false;
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.City, w => w.CityDistrict);
			base.OnChanged ();
		}
	}
}

