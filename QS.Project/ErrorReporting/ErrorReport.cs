using System;
using System.Runtime.Serialization;

namespace QS.ErrorReporting
{
	[DataContract]
	[Serializable]
	public class ErrorReport
	{
		[DataMember]
		public string Product { get; set;}

		[DataMember(IsRequired = false)]
		public string Edition { get; set;}

		[DataMember]
		public string Version { get; set;}

		[DataMember]
		public string StackTrace { get; set;}

		[DataMember]
		public string Description { get; set;}

		[DataMember]
		public string Email { get; set;}

		[DataMember]
		public string UserName { get; set;}

		[DataMember]
		public string LogFile { get; set;}
	}
}

