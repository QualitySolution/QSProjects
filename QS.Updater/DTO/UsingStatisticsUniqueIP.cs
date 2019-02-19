using System;
using System.Runtime.Serialization;

namespace QS.Updater.DTO
{
    [DataContract]
    public class UsingStatisticsUniqueIP
    {
        [DataMember]
        public string IP;

        [DataMember]
        public int Count;

        public UsingStatisticsUniqueIP(string ip, int count)
        {
            IP = ip;
            Count = count;
        }
    }
}
