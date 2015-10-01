using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Data.Bindings;

namespace QSOsm.DTO
{
	[DataContract]
	public class OsmCity
	{
		[DataMember]
		public long OsmId;
		[DataMember]
		public string Name;
		[DataMember]
		public string SuburbDistrict;
		[DataMember]
		public LocalityType LocalityType;

		public OsmCity (long osmId, string name, string suburbDistrict, LocalityType localityType)
		{
			OsmId = osmId;
			Name = name;
			SuburbDistrict = suburbDistrict;
			LocalityType = localityType;
		}

		public static LocalityType GetLocalityTypeByName (string localityName)
		{
			localityName = localityName.Trim ();
			switch (localityName) {
			case "city":
				return LocalityType.city;
			case "town":
				return LocalityType.town;
			case "village":
				return LocalityType.village;
			case "allotments":
				return LocalityType.allotments;
			case "hamlet":
				return LocalityType.hamlet;
			case "farm":
				return LocalityType.farm;
			case "isolated_dwelling":
				return LocalityType.isolated_dwelling;
			default:
				throw new NotSupportedException (String.Format ("Тип поселения \"{0}\" не поддерживается.", localityName));
			}
		}
	}

	/// <summary>
	/// Тип населенного пункта. Маленькими буквами для совпадения с соответствующими типами в OSM.
	/// </summary>
	public enum LocalityType
	{
		[ItemTitleAttribute ("Город")]
		city,
		[ItemTitleAttribute ("Город")]
		town,
		[ItemTitleAttribute ("Населенный пункт")]
		village,
		[ItemTitleAttribute ("Дачный поселок")]
		allotments,
		[ItemTitleAttribute ("Деревня")]
		hamlet,
		[ItemTitleAttribute ("Ферма")]
		farm,
		[ItemTitleAttribute ("Хутор")]
		isolated_dwelling
	}

	public class OsmCityComparer : IComparer<OsmCity>
	{
		#region IComparer implementation

		public int Compare (OsmCity x, OsmCity y)
		{
			if (x.Name.CompareTo (y.Name) != 0)
				return x.Name.CompareTo (y.Name);
			if (x.SuburbDistrict.CompareTo (y.SuburbDistrict) != 0)
				return x.SuburbDistrict.CompareTo (y.SuburbDistrict);
			return 0;
		}

		#endregion
	}
}

