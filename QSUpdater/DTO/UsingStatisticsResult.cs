using System;
using System.Runtime.Serialization;

namespace QSUpdater.DTO
{
    [DataContract]
    public class UsingStatisticsResult
    {
        [DataMember]
        public int TotalUniqueIp;

        [DataMember]
        public UsingStatisticsUniqueIP[] UniqueIPs;

        public UsingStatisticsResult()
        {
        }
    }
}
