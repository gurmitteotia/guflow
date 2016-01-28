using System.Runtime.Serialization;

namespace NetPlayground
{
    [DataContract]
    internal class ScheduleData
    {
        [DataMember]
        public string PN { get; set; }
    }
}