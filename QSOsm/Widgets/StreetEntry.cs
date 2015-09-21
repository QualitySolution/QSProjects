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
	public class StreetEntry : Entry
	{
		const int StreetColumn = 1;
		const int DistrictColumn = 2;

		private ListStore completionListStore;

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
		}

		public StreetEntry ()
		{
			Binding = new BindingControler<StreetEntry> (this, new Expression<Func<StreetEntry, object>>[] {
				(w => w.Street), (w => w.StreetDistrict)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.MatchSelected += Completion_MatchSelected;
//			this.Completion.MatchFunc = delegate(EntryCompletion completion, string key, TreeIter iter) {
//				
//			}
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			Street = args.Model.GetValue (args.Iter, StreetColumn).ToString ();
			StreetDistrict = args.Model.GetValue (args.Iter, DistrictColumn).ToString ();
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
			IOsmService svc = OsmWorker.GetOsmService ();
			var streets = svc.GetStreets (CityId);
			completionListStore = new ListStore (typeof(string), typeof(string), typeof(string));
			foreach (var s in streets) {
				completionListStore.AppendValues (
					String.IsNullOrWhiteSpace (s.District) ? s.Name : String.Format ("{0} ({1})", s.Name, s.District),
					s.Name,
					s.District
				);
			}
			this.Completion.Model = completionListStore;
			this.Completion.TextColumn = 0;
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Street, w => w.StreetDistrict);
			base.OnChanged ();
		}
	}
}

