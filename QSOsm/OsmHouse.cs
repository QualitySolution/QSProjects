using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QSOsm.DTO
{
	[DataContract]
	public class OsmHouse
	{
		[DataMember]
		public long Id;
		[DataMember]
		public string HouseNumber;
		[DataMember]
		public decimal X;
		[DataMember]
		public decimal Y;

		public OsmHouse (long id, string houseNumber, decimal x, decimal y)
		{
			Id = id;
			HouseNumber = houseNumber;
			X = x;
			Y = y;
		}
	}
}

