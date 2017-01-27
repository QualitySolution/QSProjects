using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using Newtonsoft.Json;
using QSOsm.DTO;
using QSOrmProject;

namespace QSOsm.Data
{
	public class JsonAddress : PropertyChangedBase
	{

		[Display (Name = "Регион")]
		public virtual string Region { get; set;}

		string city;

		[Display (Name = "Город")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string City {
			get { return city; }
			set { SetField (ref city, value, () => City); }
		}

		LocalityType localityType;

		[Display (Name = "Тип населенного пункта")]
		public virtual LocalityType LocalityType {
			get { return localityType; }
			set { SetField (ref localityType, value, () => LocalityType); }
		}

		string cityDistrict;

		[Display (Name = "Район области")]
		public virtual string CityDistrict {
			get { return cityDistrict; }
			set { SetField (ref cityDistrict, value, () => CityDistrict); }
		}

		string street;

		[Display (Name = "Улица")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string Street {
			get { return street; }
			set { SetField (ref street, value, () => Street); }
		}

		string streetDistrict;

		[Display (Name = "Район города")]
		public virtual string StreetDistrict {
			get { return streetDistrict; }
			set { SetField (ref streetDistrict, value, () => StreetDistrict); }
		}
			
		string building;

		[Display (Name = "Номер дома")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string Building {
			get { return building; }
			set { SetField (ref building, value, () => Building); }
		}

		string letter;

		[Display (Name = "Литера")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string Letter {
			get { return letter; }
			set { SetField (ref letter, value, () => Letter); }
		}

		int floor;

		[Display (Name = "Этаж")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual int Floor {
			get { return floor; }
			set { SetField (ref floor, value, () => Floor); }
		}

		string room;

		[Display (Name = "Офис/Квартира")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string Room {
			get { return room; }
			set { SetField (ref room, value, () => Room); }
		}

		RoomType roomType;

		[Display (Name = "Тип помещения")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual RoomType RoomType {
			get { return roomType; }
			set { SetField (ref roomType, value, () => RoomType); }
		}

		string addressAddition;

		[Display (Name = "Дополнение к адресу")]
		[PropertyChangedAlso("CompiledAddress", "ShortAddress", "Title")]
		public virtual string АddressAddition {
			get { return addressAddition; }
			set { SetField (ref addressAddition, value, () => АddressAddition); }
		}

		#region Расчетные

		[Display (Name = "Полный адрес")]
		public virtual string CompiledAddress {
			get {
				string address = String.Empty;
				if (!String.IsNullOrWhiteSpace (City))
					address += String.Format ("{0} {1}, ", LocalityType.GetEnumShortTitle(), City);
				if (!String.IsNullOrWhiteSpace (Street))
					address += String.Format ("{0}, ", Street);
				if (!String.IsNullOrWhiteSpace (Building))
					address += String.Format ("д.{0}, ", Building);
				if (!String.IsNullOrWhiteSpace (Letter))
					address += String.Format ("лит.{0}, ", Letter);
				if (default(int) != Floor)
					address += String.Format ("эт.{0}, ", Floor);
				if (!String.IsNullOrWhiteSpace (Room))
					address += String.Format ("{0} {1}, ", RoomType.GetEnumShortTitle(), Room);
				if (!String.IsNullOrWhiteSpace (АddressAddition))
					address += String.Format ("{0}, ", АddressAddition);

				return address.TrimEnd (',', ' ');
			}
		}

		[Display (Name = "Сокращенный адрес")]
		public virtual string ShortAddress {
			get {
				string address = String.Empty;
				if (!String.IsNullOrWhiteSpace (City) && City != "Санкт-Петербург")
					address += String.Format ("{0} {1}, ", LocalityType.GetEnumShortTitle() , AddressHelper.ShortenCity(City));
				if (!String.IsNullOrWhiteSpace (Street))
					address += String.Format ("{0}, ", AddressHelper.ShortenStreet(Street));
				if (!String.IsNullOrWhiteSpace (Building))
					address += String.Format ("д.{0}, ", Building);
				if (!String.IsNullOrWhiteSpace (Letter))
					address += String.Format ("лит.{0}, ", Letter);
				if (default(int) != Floor)
					address += String.Format ("эт.{0}, ", Floor);
				if (!String.IsNullOrWhiteSpace (Room))
					address += String.Format ("{0} {1}, ", RoomType.GetEnumShortTitle(), Room);

				return address.TrimEnd (',', ' ');
			}
		}

		[JsonIgnore]
		public virtual string Title { 
			get { return String.IsNullOrWhiteSpace(CompiledAddress) ? "АДРЕС ПУСТОЙ" : CompiledAddress; }
		}

		#endregion

		public JsonAddress()
		{
		}
	}
}

