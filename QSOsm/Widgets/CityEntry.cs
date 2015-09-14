using System;
using Gtk;
using Gamma.Binding.Core;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class CityEntry : Entry
	{
		private ListStore completionListStore;

		private delegate List<OsmCity> GetCitiesDelegate ();

		private bool queryIsRunning = false;

		public BindingControler<CityEntry> Binding { get; private set; }

		public CityEntry ()
		{
			Binding = new BindingControler<CityEntry> (this, new Expression<Func<CityEntry, object>>[] {
				(w => w.Text)
			});
			this.Completion = new EntryCompletion ();
			this.TextInserted += CityEntryTextInserted;
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
			completionListStore = new ListStore (typeof(long), typeof(string));
			foreach (var city in cities) {
				completionListStore.AppendValues (
					city.OsmId, 
					String.IsNullOrWhiteSpace (city.SuburbDistrict) ? city.Name : String.Format ("{0} ({1})", city.Name, city.SuburbDistrict));
			}
			this.Completion.Model = completionListStore;
			this.Completion.TextColumn = 1;
			queryIsRunning = false;
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.Text);
			base.OnChanged ();
		}
	}
}

