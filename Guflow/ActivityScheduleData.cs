using System.Runtime.Serialization;

namespace Guflow
{
    [DataContract]
    internal class ActivityScheduleData
    {
        [DataMember]
        public string PN { get; set; }
    }
}