using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gamma.Utilities;
using QS.Osm.Data;
using QS.Osm.DTO;
using QSOrmProject;

namespace QSOsm
{
	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public partial class JsonAddressEntry : Gtk.Bin
	{
		public BindingControler<JsonAddressEntry> Binding { get; private set; }

		JsonAddress address;
		public JsonAddress Address
		{
			get
			{
				return address;
			}
			set
			{
				if(address != null)
				{
					entryCity.Binding.CleanSources();
					entryStreet.Binding.CleanSources();
					entryBuilding.Binding.CleanSources();
					yentryLiter.Binding.CleanSources();
					comboRoomType.Binding.CleanSources();
					entryRoom.Binding.CleanSources();
					spinFloor.Binding.CleanSources();
					yentryAddition.Binding.CleanSources();
					Address.PropertyChanged -= Address_PropertyChanged;
				}
				address = value;
				if(Address != null)
				{
					ExpanderLabel.LabelProp = Address.Title;
					Address.PropertyChanged += Address_PropertyChanged;

					var nullConverter = new NullToEmptyStringConverter();
					entryCity.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.CityDistrict, widget => widget.CityDistrict, nullConverter)
						.AddBinding (entity => entity.City, widget => widget.City, nullConverter)
						.AddBinding (entity => entity.LocalityType, widget => widget.Locality)
						.InitializeFromSource ();
					entryStreet.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.StreetDistrict, widget => widget.StreetDistrict, nullConverter)
						.AddBinding (entity => entity.Street, widget => widget.Street, nullConverter)
						.InitializeFromSource ();
					entryBuilding.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.Building, widget => widget.House, nullConverter)
						.InitializeFromSource ();

					yvalidatedentryPostalCode.ValidationMode = QSWidgetLib.ValidationType.numeric;
					yvalidatedentryPostalCode.Binding.AddBinding(Address, e => e.PostalCode, w => w.Text, nullConverter).InitializeFromSource();

					comboRoomType.ItemsEnum = typeof(RoomType);
					comboRoomType.Binding.AddBinding (Address, entity => entity.RoomType, widget => widget.SelectedItem)
						.InitializeFromSource ();

					yentryLiter.Binding.AddBinding(Address, e => e.Letter, w => w.Text, nullConverter).InitializeFromSource();
					entryRoom.Binding.AddBinding(Address, e => e.Room, w => w.Text, nullConverter).InitializeFromSource();
					spinFloor.Binding.AddBinding(Address, e => e.Floor, w => w.ValueAsInt, new NullToZeroConverter()).InitializeFromSource();
					yentryAddition.Binding.AddBinding(Address, e => e.АddressAddition, w => w.Text, nullConverter).InitializeFromSource();
				}
			}
		}

		void Address_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Address.GetPropertyName(x => x.Title))
				ExpanderLabel.LabelProp = Address.Title;
		}

		[Display (Name = "Регион")]
		public virtual string Region {
			get { return null; }
			set { ; }
		}

		[Display (Name = "Город")]
		public virtual string City {
			get { return entryCity.City; }
			set { entryCity.City = value; }
		}

		[Display (Name = "Район области")]
		public virtual string CityInDistrict {
			get { return entryCity.CityDistrict; }
			set { entryCity.CityDistrict = value; }
		}

		[Display (Name = "Улица")]
		public virtual string Street {
			get { return entryStreet.Street; }
			set { entryStreet.Street = value; }
		}

		[Display (Name = "Район города")]
		public virtual string StreetInDistrict {
			get { return entryStreet.StreetDistrict; }
			set { entryStreet.StreetDistrict = value; }
		}

		[Display (Name = "Номер дома")]
		public virtual string Building {
			get { return entryBuilding.House; }
			set { entryBuilding.House = value; }
		}

		bool hideFloor;

		public virtual bool HideFloor {
			get { return hideFloor; }
			set { hideFloor = value;
				spinFloor.Visible = labelFloor.Visible = !value; }
		}

		public JsonAddressEntry ()
		{
			this.Build ();

			Binding = new BindingControler<JsonAddressEntry> (this, new Expression<Func<JsonAddressEntry, object>>[] {
				(w => w.Region),
				(w => w.CityInDistrict),
				(w => w.City),
				(w => w.Street), 
				(w => w.StreetInDistrict),
				(w => w.Binding),
				(w => w.Address)
			});

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityId = entryCity.OsmId;
			};

			entryStreet.StreetSelected += (sender, e) => {
				entryBuilding.Street = new OsmStreet (-1, entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

			expander1.Expanded = false;
		}

		protected void OnButtonCleanClicked(object sender, EventArgs e)
		{
			Address.Clean();
		}

		protected void OnSpinFloorShown(object sender, EventArgs e)
		{
			if (HideFloor)
				spinFloor.Visible = false;
		}

		protected void OnLabelFloorShown(object sender, EventArgs e)
		{
			if (HideFloor)
				labelFloor.Visible = false;
		}
	}
}

