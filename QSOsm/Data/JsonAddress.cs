using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using Newtonsoft.Json;
using QSOsm.DTO;

namespace QSOsm.Data
{
	public class JsonAddress
	{

		[Display (Name = "Регион")]
		public virtual string Region { get; set;}

		[Display (Name = "Город")]
		public virtual string City { get; set;}

		[Display (Name = "Тип населенного пункта")]
		public virtual LocalityType LocalityType { get; set;}

		[Display (Name = "Район области")]
		public virtual string CityInDistrict { get; set;}

		[Display (Name = "Улица")]
		public virtual string Street { get; set;}

		[Display (Name = "Район города")]
		public virtual string StreetInDistrict { get; set;}

		[Display (Name = "Номер дома")]
		public virtual string Building { get; set;}

		[Display (Name = "Литера")]
		public virtual string Letter { get; set;}

		[Display (Name = "Этаж")]
		public virtual int Floor { get; set;}

		[Display (Name = "Помещение")]
		public virtual string Room { get; set;}

		[Display (Name = "Тип помещения")]
		public virtual RoomType RoomType { get; set;}

		[Display (Name = "Дополнение")]
		public virtual string АddressAddition { get; set;}

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

