using System;
using System.Runtime.Serialization;

namespace QS.Updater.DTO
{

	[DataContract]
	public class VersionRequests
	{
		[DataMember]
		public string Version;

		[DataMember]
		public string Modification;

		[DataMember]
		public int Count;

		public VersionRequests(string version, string modification, int count)
		{
			Version = version;
			Modification = modification;
			Count = count;
		}
	}
}
