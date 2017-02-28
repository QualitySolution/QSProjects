using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSOsm.Data;
using QSOsm.DTO;
using Gamma.Utilities;

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

					entryCity.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.CityDistrict, widget => widget.CityDistrict)
						.AddBinding (entity => entity.City, widget => widget.City)
						.AddBinding (entity => entity.LocalityType, widget => widget.Locality)
						.InitializeFromSource ();
					entryStreet.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.StreetDistrict, widget => widget.StreetDistrict)
						.AddBinding (entity => entity.Street, widget => widget.Street)
						.InitializeFromSource ();
					entryBuilding.Binding
						.AddSource (Address)
						.AddBinding (entity => entity.Building, widget => widget.House)
						.InitializeFromSource ();

					yvalidatedentryPostalCode.ValidationMode = QSWidgetLib.ValidationType.numeric;
					yvalidatedentryPostalCode.Binding.AddBinding(Address, e => e.PostalCode, w => w.Text).InitializeFromSource();

					comboRoomType.ItemsEnum = typeof(RoomType);
					comboRoomType.Binding.AddBinding (Address, entity => entity.RoomType, widget => widget.SelectedItem)
						.InitializeFromSource ();

					yentryLiter.Binding.AddBinding(Address, e => e.Letter, w => w.Text).InitializeFromSource();
					entryRoom.Binding.AddBinding(Address, e => e.Room, w => w.Text).InitializeFromSource();
					spinFloor.Binding.AddBinding(Address, e => e.Floor, w => w.ValueAsInt).InitializeFromSource();
					yentryAddition.Binding.AddBinding(Address, e => e.АddressAddition, w => w.Text).InitializeFromSource();
				}
			}
		}

		void Address_PropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == Address.GetPropertyName(x => x.CompiledAddress))
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
	}
}

