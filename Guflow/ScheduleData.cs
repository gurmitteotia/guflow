using System.Runtime.Serialization;

namespace Guflow
{
    [DataContract]
    internal class ScheduleData
    {
        [DataMember]
        public string PN { get; set; }
    }
}