using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSOsm.DTO;

namespace QSOsm
{
	//FIXME Начал перенос всего адреса в отдельный вижжет но понял что неудачное решение для точек доставки.

	[System.ComponentModel.ToolboxItem (true)]
	[System.ComponentModel.Category ("Gamma OSM Widgets")]
	public partial class FullAddressEntry : Gtk.Bin
	{
		public BindingControler<FullAddressEntry> Binding { get; private set; }

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

		public FullAddressEntry ()
		{
			this.Build ();

			Binding = new BindingControler<FullAddressEntry> (this, new Expression<Func<FullAddressEntry, object>>[] {
				(w => w.Region),
				(w => w.CityInDistrict),
				(w => w.City),
				(w => w.Street), 
				(w => w.StreetInDistrict),
				(w => w.Binding)
			});

			entryCity.CitySelected += (sender, e) => {
				entryStreet.CityId = entryCity.OsmId;
			};

			entryStreet.StreetSelected += (sender, e) => {
				entryBuilding.Street = new OsmStreet (entryStreet.CityId, entryStreet.Street, entryStreet.StreetDistrict);
			};

		}
	}
}

