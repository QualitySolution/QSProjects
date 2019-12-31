using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.DTO;
using Gamma.Utilities;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class CityEntry : Entry
	{
		enum columns
		{
			City,
			District,
			Locality,
			OsmId
		}

		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

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
				if (String.IsNullOrWhiteSpace(city))
					osmId = default(long);
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

		LocalityType locality;

		public LocalityType Locality {
			get { 
				return locality;
			}
			set {
				locality = value;
				UpdateText ();
			}
		}


		void UpdateText ()
		{
			if (String.IsNullOrWhiteSpace(City))
				Text = String.Empty;
			else if (String.IsNullOrWhiteSpace(CityDistrict))
				Text = String.Format("{0} {1}", Locality.GetEnumTitle(), City);
			else
				Text = String.Format ("{0} {1} ({2})", Locality.GetEnumTitle (), City, CityDistrict);
			
			if (osmId == default(long) && City != default(string)) {
				logger.Debug ("Запрос id для города {0}({1})...", City, CityDistrict);
				IOsmService svc = OsmWorker.GetOsmService ();
				if (svc == null) {
					logger.Warn ("Не удалось получить id города.");
					return;
				}
				osmId = svc.GetCityId (City, CityDistrict, Locality.ToString ());
				logger.Debug ("id={0}", osmId);
				OnCitySelected ();
			}
		}

		public CityEntry ()
		{
			Binding = new BindingControler<CityEntry> (this, new Expression<Func<CityEntry, object>>[] {
				(w => w.City), (w => w.CityDistrict), (w => w.Locality)
			});

			this.TextInserted += CityEntryTextInserted;

			this.Completion = new EntryCompletion ();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;

			this.Completion.MatchFunc = Completion_MatchFunc;
			var cell = new CellRendererText ();
			this.Completion.PackStart (cell, true);
			this.Completion.SetCellDataFunc (cell, OnCellLayoutDataFunc);
		}

		//Костыль, для отображения выпадающего списка
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Control_R)
				this.InsertText("");

			return base.OnKeyPressEvent(evnt);
		}

		void OnCellLayoutDataFunc (CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var cityName = (string)tree_model.GetValue (iter, (int)columns.City);
			var district = (string)tree_model.GetValue (iter, (int)columns.District);
			var localityType = (LocalityType)(tree_model.GetValue (iter, (int)columns.Locality) ?? default(LocalityType));
			string pattern = String.Format ("\\b{0}", Regex.Escape (Text.ToLower ()));
			cityName = Regex.Replace (cityName, pattern, (match) => String.Format ("<b>{0}</b>", match.Value), RegexOptions.IgnoreCase);
			(cell as CellRendererText).Markup = String.IsNullOrWhiteSpace (district) ? 
				String.Format ("{0} {1}", localityType.GetEnumTitle (), cityName) : 
				String.Format ("{0} {1} ({2})", localityType.GetEnumTitle (), cityName, district);
		}

		bool Completion_MatchFunc (EntryCompletion completion, string key, TreeIter iter)
		{
			var val = completion.Model.GetValue (iter, (int)columns.City).ToString ().ToLower ();
			return Regex.IsMatch (val, String.Format ("\\b{0}.*", Regex.Escape (this.Text.ToLower ())));
		}

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			city = args.Model.GetValue (args.Iter, (int)columns.City).ToString ();
			cityDistrict = args.Model.GetValue (args.Iter, (int)columns.District).ToString ();
			locality = (LocalityType)args.Model.GetValue (args.Iter, (int)columns.Locality);
			UpdateText ();
			osmId = (long)args.Model.GetValue (args.Iter, (int)columns.OsmId);
			OnCitySelected ();
			args.RetVal = true;
		}

		protected virtual void OnCitySelected ()
		{
			if (CitySelected != null)
				CitySelected (null, EventArgs.Empty);
		}

		void CityEntryTextInserted (object o, TextInsertedArgs args)
		{
			if (this.HasFocus && completionListStore == null && !queryIsRunning) {
				Thread queryThread = new Thread (fillAutocomplete);
				queryThread.IsBackground = true;
				queryIsRunning = true;
				queryThread.Start ();
			}
		}

		private void fillAutocomplete ()
		{
			logger.Info ("Запрос городов...");
			IOsmService svc = OsmWorker.GetOsmService ();
			var cities = svc.GetCities ();
			completionListStore = new ListStore (typeof(string), typeof(string), typeof(LocalityType), typeof(long));
			foreach (var c in cities) {
				completionListStore.AppendValues (
					c.Name,
					c.SuburbDistrict,
					c.LocalityType,
					c.OsmId
				);
			}

			try {
				this.Completion.Model = completionListStore;
				queryIsRunning = false;
				if(this.HasFocus)
					this.Completion.Complete();
				logger.Info("Получено {0} городов", cities.Count);
			} 
			catch {
				logger.Info("Не получилось отобразить автодополнение. Возможно {0} уже был удалён", this.Name);
			}
		}

		protected override void OnChanged ()
		{
			Binding.FireChange (w => w.City, w => w.CityDistrict, w => w.Locality);
			base.OnChanged ();
		}
	}
}
