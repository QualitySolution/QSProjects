using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.DTO;
using QSProjectsLib;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class HouseEntry : Entry
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		public event EventHandler CompletionLoaded;

		private ListStore completionListStore;

		private Thread queryThread;

		public BindingControler<HouseEntry> Binding { get; private set; }

		public bool? OsmCompletion { 
			get {
				if (completionListStore == null)
					return null;
				return completionListStore.Cast<object[]> ().Any (row => (string)row [0] == House);
			}
		}

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

		public OsmHouse OsmHouse { get; private set;}

		public virtual decimal? Latitude {
			get { decimal? latitude, longitude;
				GetCoordinates(out longitude, out latitude);
				return latitude; 
			}
		}

		public virtual decimal? Longitude {
			get { decimal? latitude, longitude;
				GetCoordinates(out longitude, out latitude);
				return longitude; }
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
				(w => w.House),
				(w => w.OsmCompletion),
				(w => w.Text),
				(w => w.Latitude),
				(w => w.Longitude)
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
			
		//Костыль, для отображения выпадающего списка
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Control_R)
				this.InsertText("");
			
			return base.OnKeyPressEvent(evnt);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = (string)completion.Model.GetValue (iter, 0);
			return Regex.IsMatch (val, String.Format ("{0}", Regex.Escape (this.Text)), RegexOptions.IgnoreCase);
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			House = args.Model.GetValue (args.Iter, 0).ToString ();
			args.RetVal = true;
		}
			
		void OnStreetSet ()
		{
			if (queryThread != null && queryThread.IsAlive) {
				try {
					queryThread.Abort ();
				} catch (ThreadAbortException ex) {
					logger.Warn ("fillAutocomplete() thread for houses was aborted.");
				}
			}
			queryThread = new Thread (fillAutocomplete);
			queryThread.IsBackground = true;
			queryThread.Start ();
		}

		private bool entryDestroyed = false;

		private void fillAutocomplete ()
		{
			if (String.IsNullOrWhiteSpace (Street.Name)) {
				if (completionListStore != null)
					completionListStore.Clear ();				
			} else {
				
				logger.Info ("Запрос домов на {0}...", Street.Name);
				IOsmService svc = OsmWorker.GetOsmService ();

				List<OsmHouse> houses;
				if (Street.Districts == null)
					houses = svc.GetHouseNumbersWithoutDistrict (Street.CityId, Street.Name);
				else
					houses = svc.GetHouseNumbers (Street.CityId, Street.Name, Street.Districts);

				//Удаляем литеры А у домов где других литер нет.
				foreach(var house in houses.Where(x => x.Letter == "А" || x.Letter == "а" || x.Letter == "A" || x.Letter == "a"))
				{
					if (!houses.Any(x => x.HouseNumber == house.HouseNumber && x.Letter != house.Letter))
						house.Letter = String.Empty;
				}
				houses = houses.OrderBy(x => x.ComplexNumber, new StringWorks.NaturalStringComparerNonStatic()).ToList();

				completionListStore = new ListStore (typeof(string), typeof(long), typeof(OsmHouse));
				foreach (var h in houses) {
					completionListStore.AppendValues (h.ComplexNumber, h.Id, h);
				}
				if(entryDestroyed) {
					logger.Info("Запрос домов на {0} отменён", Street.Name);
					return;
				}

				this.Completion.Model = completionListStore;
				logger.Info ("Получено {0} домов", houses.Count);
			}
			if (CompletionLoaded != null)
				Gtk.Application.Invoke (CompletionLoaded);
		}

		public void GetCoordinates(out decimal? longitude, out decimal? latitude)
		{
			longitude = null;
			latitude = null;
			var osmRow = completionListStore == null ? null : completionListStore.Cast<object[]>().FirstOrDefault(row => (string)row[0] == House);
			if (osmRow == null)
				return;

			var osmhouse = (OsmHouse)osmRow[2];
			longitude = osmhouse.X;
			latitude = osmhouse.Y;
		}

		protected override void OnChanged ()
		{
			if(completionListStore != null)
			{
				var houserow = completionListStore.Cast<object[]>().FirstOrDefault(row => (string)row[0] == Text);
				if(houserow != null)
					OsmHouse = houserow[2] as OsmHouse;
			}

			Binding.FireChange (w => w.House, w => w.Text, w => w.OsmCompletion, w => w.Latitude, w => w.Longitude);
			base.OnChanged ();
		}

		protected override void OnDestroyed()
		{
			entryDestroyed = true;
			base.OnDestroyed();
		}
	}
}

