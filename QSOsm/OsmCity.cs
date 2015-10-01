using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
	}

	/// <summary>
	/// Тип населенного пункта. Маленькими буквами для совпадения с соответствующими типами в OSM.
	/// </summary>
	public enum LocalityType
	{
		[Display (Name = "Город")]
		city,
		[Display (Name = "Малый город")]
		town,
		[Display (Name = "Населенный пункт")]
		village,
		[Display (Name = "Дачный поселок")]
		allotments,
		[Display (Name = "Деревня")]
		hamlet,
		[Display (Name = "Ферма")]
		farm,
		[Display (Name = "Хутор")]
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

