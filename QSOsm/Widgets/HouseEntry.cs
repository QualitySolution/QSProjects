using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class HouseEntry : Entry
	{
		private ListStore completionListStore;

		private Thread queryThread;

		public BindingControler<HouseEntry> Binding { get; private set; }

		string house;

		public string House {
			get {
				return house;
			}
			set {
				house = value;
				this.Text = House;
			}
		}

		OsmStreet street;

		public OsmStreet Street {
			get { 
				return street;
			}
			set {
				street = value;
				OnStreetSet ();
			}
		}

		public HouseEntry ()
		{
			Binding = new BindingControler<HouseEntry> (this, new Expression<Func<HouseEntry, object>>[] {
				(w => w.House)
			});

			this.Completion = new EntryCompletion ();
			this.Completion.TextColumn = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;
			this.Completion.MatchFunc = Completion_MatchFunc;
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = (string)completion.Model.GetValue (iter, 0);
			return Regex.IsMatch (val, String.Format ("{0}", Regex.Escape (key)), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			House = args.Model.GetValue (args.Iter, 0).ToString ();
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
			IOsmService svc = OsmWorker.GetOsmService ();
			var houses = svc.GetHouseNumbers (Street.CityOsmId, Street.Name, Street.District);
			completionListStore = new ListStore (typeof(string));
			foreach (var h in houses) {
				completionListStore.AppendValues (h);
			}
			this.Completion.Model = completionListStore;
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.House);
			base.OnChanged ();
		}
	}
}

