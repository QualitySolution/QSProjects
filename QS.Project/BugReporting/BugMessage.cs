using System;
using System.Runtime.Serialization;

namespace QS.BugReporting
{
	[DataContract]
	[Serializable]
	public class BugMessage
	{
		[DataMember]
		public string product { get; set;}

		[DataMember(IsRequired = false)]
		public string Edition { get; set;}

		[DataMember]
		public string version { get; set;}

		[DataMember]
		public string stackTrace { get; set;}

		[DataMember]
		public string description { get; set;}

		[DataMember]
		public string email { get; set;}

		[DataMember]
		public string userName { get; set;}

		[DataMember]
		public string logFile { get; set;}
	}
}

