using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Gamma.Binding.Core;
using Gtk;
using QSOsm.DTO;
using Gamma.Utilities;
using QSOsm.Loaders;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public class CityEntry : Entry
	{
		private ICitiesDataLoader citiesDataLoader;
		public ICitiesDataLoader CitiesDataLoader {
			get { return citiesDataLoader; }
			set { ChangeDataLoader(citiesDataLoader, value); }
		}

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

		public BindingControler<CityEntry> Binding { get; private set; }

		public long OsmId { get; private set; }

		string city;

		public string City {
			get {
				return city;
			}
			set {
				city = value;
				if (String.IsNullOrWhiteSpace(city))
					OsmId = default(long);
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
				Text = String.Format("{0} {1} ({2})", Locality.GetEnumTitle(), City, CityDistrict);

			if(CitiesDataLoader == null) {
				logger.Warn($"Невозможно получить данные с сервера тк поле {nameof(citiesDataLoader)} не заполнено");
				return;
			}

			if(OsmId == default(long) && City != default(string)) {
				OsmId = citiesDataLoader.GetCityId(city, CityDistrict, Locality);
				logger.Debug("id={0}", OsmId);
				OnCitySelected();
			}
		}


		public void TextUpdated(object o, TextInsertedArgs args)
		{
			citiesDataLoader.LoadCities(Text);
		}

		public void TextUpdated(object o, TextDeletedArgs args) => TextUpdated(o, TextInsertedArgs.Empty as TextInsertedArgs);

		public CityEntry ()
		{
			Binding = new BindingControler<CityEntry>(this, new Expression<Func<CityEntry, object>>[] {
				(w => w.City), (w => w.CityDistrict), (w => w.Locality)
			});

			this.Completion = new EntryCompletion();
			this.Completion.MinimumKeyLength = 0;
			this.Completion.MatchSelected += Completion_MatchSelected;

			this.Completion.MatchFunc = (completion, key, iter) => true;
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

		[GLib.ConnectBefore]
		void Completion_MatchSelected (object o, MatchSelectedArgs args)
		{
			city = args.Model.GetValue (args.Iter, (int)columns.City).ToString ();
			cityDistrict = args.Model.GetValue (args.Iter, (int)columns.District).ToString ();
			locality = (LocalityType)args.Model.GetValue (args.Iter, (int)columns.Locality);
			UpdateText ();
			OsmId = (long)args.Model.GetValue (args.Iter, (int)columns.OsmId);
			OnCitySelected ();
			args.RetVal = true;
		}

		protected virtual void OnCitySelected()
		{
			CitySelected?.Invoke(null, EventArgs.Empty);
		}

		private void ChangeDataLoader(ICitiesDataLoader oldValue, ICitiesDataLoader newValue)
		{
			if(oldValue == newValue)
				return;
			if(oldValue != null) {
				oldValue.CitiesLoaded -= CitiesLoaded;
				this.TextInserted -= TextUpdated;
				this.TextDeleted -= TextUpdated;
			}

			citiesDataLoader = newValue;
			if(CitiesDataLoader == null)
				return;
			CitiesDataLoader.CitiesLoaded += CitiesLoaded;
			this.TextInserted += TextUpdated;
			this.TextDeleted += TextUpdated;
		}

		private void CitiesLoaded()
		{
			Application.Invoke((senderObject, eventArgs) => {
				OsmCity[] cities = citiesDataLoader.GetCities();
				completionListStore = new ListStore(typeof(string), typeof(string), typeof(LocalityType), typeof(long));
				foreach(var c in cities) {
					completionListStore.AppendValues(
						c.Name,
						c.SuburbDistrict,
						c.LocalityType,
						c.OsmId
					);
				}
				Completion.Model = completionListStore;
				if(HasFocus)
					Completion?.Complete();
			});
		}

		protected override void OnChanged()
		{
			Binding.FireChange (w => w.City, w => w.CityDistrict, w => w.Locality);
			base.OnChanged ();
		}

		protected override void OnDestroyed()
		{
			if(CitiesDataLoader != null)
				citiesDataLoader.CitiesLoaded -= CitiesLoaded;
			base.OnDestroyed();
		}
	}
}
