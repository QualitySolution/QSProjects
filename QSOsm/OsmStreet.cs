using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QSOsm
{
	[DataContract]
	public class OsmStreet
	{
		[DataMember]
		public long CityOsmId;
		[DataMember]
		public string Name;
		[DataMember]
		public string District;

		public OsmStreet (long cityId, string name, string district)
		{
			CityOsmId = cityId;
			Name = name;
			District = district;
		}
	}

	public class OsmStreetComparer : IComparer<OsmStreet>
	{
		#region IComparer implementation

		public int Compare (OsmStreet x, OsmStreet y)
		{
			if (x.Name.CompareTo (y.Name) != 0)
				return x.Name.CompareTo (y.Name);
			if (x.District.CompareTo (y.District) != 0)
				return x.District.CompareTo (y.District);
			return 0;
		}

		#endregion
	}
}

