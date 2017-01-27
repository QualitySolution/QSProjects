using System;
using System.Collections.Generic;
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

