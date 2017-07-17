using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QSTelemetry
{
	[DataContract]
	[Serializable]
    public class TelemetryStatistic
    {
        [DataMember(IsRequired = false)]
        public long? UpdateStatisticId{ get; set; }

        [DataMember]
		public string Product { get; set; }

        [DataMember]
		public string Edition { get; set; }

		[DataMember]
		public string Version { get; set; }

		[DataMember]
		public string OS { get; set; }

		[DataMember]
		public string NetFramework { get; set; }

		[DataMember]
        public bool IsDemo { get; set; }

		[DataMember]
        public Dictionary<string, long> Counters { get; set; }

		public TelemetryStatistic()
        {
        }
    }
}
