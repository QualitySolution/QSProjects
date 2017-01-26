using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QSOsm.DTO
{
	[DataContract]
	public class OsmStreet
	{
		[DataMember]
		public long Id;
		[DataMember]
		public long CityId;
		[DataMember]
		public string Name;
		[DataMember]
		public string Districts;

		public OsmStreet (long id, long cityId, string name, string districts)
		{
			Id = id;
			CityId = cityId;
			Name = name;
			Districts = districts;
		}
	}

	public class OsmStreetComparer : IComparer<OsmStreet>
	{
		#region IComparer implementation

		public int Compare (OsmStreet x, OsmStreet y)
		{
			return x.Name.CompareTo (y.Name);
		}

		#endregion
	}
}

