using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QS.Osm.DTO
{
	[DataContract]
	public class OsmHouse
	{
		[DataMember]
		public long Id;
		[DataMember]
		public string HouseNumber;
		[DataMember]
		public string Letter;
		[DataMember]
		public string Name;
		[DataMember]
		public decimal X;
		[DataMember]
		public decimal Y;

		public OsmHouse (long id, string houseNumber, string letter, string name, decimal x, decimal y)
		{
			Id = id;
			HouseNumber = houseNumber;
			Letter = letter;
			Name = name;
			X = x;
			Y = y;
		}

		public string ComplexNumber{
			get{
				if (String.IsNullOrWhiteSpace(Letter) || HouseNumber.Contains("лит") || HouseNumber.Contains("Лит")
					|| HouseNumber.Contains(Letter))
					return HouseNumber;

				return String.Format("{0} лит. {1}", HouseNumber, Letter);
			}
		}
	}
}

